using System;
using System.Buffers;

namespace Pitchfork.SemanticVersioning
{
    // https://semver.org/#backusnaur-form-grammar-for-valid-semver-versions
    internal ref struct SemVerV2BnfParser
    {
#if NET8_0_OR_GREATER
        private static readonly SearchValues<char> _allAlphanumericChars = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-");
#endif

        public SemVerV2BnfParser(ReadOnlySpan<char> input)
        {
            Remaining = input;
        }

        public readonly bool IsEof => Remaining.IsEmpty;

        public ReadOnlySpan<char> Remaining { readonly get; private set; }

        // consume [A-Za-z0-9-] with no restrictions
        private bool TryConsumeAlphanumericChars()
        {
#if NET8_0_OR_GREATER
            int i = Remaining.IndexOfAnyExcept(_allAlphanumericChars);
            if (i < 0) { i = Remaining.Length; }
#else
            int i = 0;
            for (; i < Remaining.Length; i++)
            {
                if (Remaining[i] is not ((>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '-'))
                {
                    break;
                }
            }
#endif

            Remaining = Remaining[i..];
            return (i > 0);
        }

        // <alphanumeric identifier> ::= <non-digit> | <non-digit> <identifier characters> | <identifier characters> <non-digit> | <identifier characters> <non-digit> <identifier characters>
        // <identifier characters> ::= <identifier character> | <identifier character> <identifier characters>
        public bool TryConsumeAlphanumericIdentifier()
        {
            // Basically, [A-Za-z0-9-] as long as it contains at least one non-digit.
            // We'll strip off all the digit-only chars, then we'll consume all
            // remaining alphanumeric chars. If the number of alphanumeric chars
            // we strip off is non-zero, then we found a well-formed identifier.

            SemVerV2BnfParser copy = this; // don't write back to the current instance until operation completes successfully

            copy.TryConsumeDigits();
            if (!copy.TryConsumeAlphanumericChars())
            {
                return false;
            }

            this = copy;
            return true;
        }

        // <build> ::= <dot-separated build identifiers>
        public bool TryConsumeBuild()
        {
            SemVerV2BnfParser copy = this; // don't write back to the current instance until operation completes successfully

            do
            {
                // <build identifier> ::= <alphanumeric identifier> | <digits>
                // this really means [A-Za-z0-9-] with no restrictions
                if (!copy.TryConsumeAlphanumericChars())
                {
                    return false;
                }
            } while (copy.TryConsumeLiteralChar('.'));

            this = copy;
            return true;
        }

        // <digits> ::= <digit> | <digit> <digits>
        private bool TryConsumeDigits()
        {
            // this really means [0-9] with no restrictions

#if NET8_0_OR_GREATER
            int i = Remaining.IndexOfAnyExceptInRange('0', '9');
            if (i < 0) { i = Remaining.Length; }
#else
            int i = 0;
            for (; i < Remaining.Length; i++)
            {
                if (Remaining[i] is not (>= '0' and <= '9'))
                {
                    break;
                }
            }
#endif

            Remaining = Remaining[i..];
            return (i > 0);
        }

        public bool TryConsumeLiteralChar(char ch)
        {
            if (!Remaining.IsEmpty && Remaining[0] == ch)
            {
                Remaining = Remaining[1..];
                return true;
            }

            return false;
        }

        // <numeric identifier> ::= "0" | <positive digit> | <positive digit> <digits>
        public bool TryConsumeNumericIdentifier()
        {
            // order matters below since evaluation has side effects and we don't want to consume "01" (leading zero)
            return TryConsumeLiteralChar('0') || TryConsumeDigits();
        }

        // <pre-release> ::= <dot-separated pre-release identifiers>
        public bool TryConsumePrerelease()
        {
            SemVerV2BnfParser copy = this; // don't write back to the current instance until operation completes successfully

            do
            {
                // order matters below since evaluation has side effects and we want "0-0" to be seen as alphanumeric, not numeric
                if (!copy.TryConsumeAlphanumericIdentifier() && !copy.TryConsumeNumericIdentifier())
                {
                    return false;
                }
            } while (copy.TryConsumeLiteralChar('.'));

            this = copy;
            return true;
        }
    }
}
