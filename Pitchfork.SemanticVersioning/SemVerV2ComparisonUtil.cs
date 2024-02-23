using System;
using System.Diagnostics;

namespace Pitchfork.SemanticVersioning
{
    internal static class SemVerV2ComparisonUtil
    {
        internal static int CompareMajorMinorPatch(SemanticVersion left, SemanticVersion right)
        {
            int result = CompareNumericIdentifiers(left.Major, right.Major);
            if (result != 0) { return result; }

            result = CompareNumericIdentifiers(left.Minor, right.Minor);
            if (result != 0) { return result; }

            return CompareNumericIdentifiers(left.Patch, right.Patch);
        }

        internal static int ComparePrerelease(SemanticVersion left, SemanticVersion right)
        {
            // Split into individual identifiers and compare them.
            // Given two semver strings, if only one has a prerelease component, it always
            // sorts before the string without a prerelease component.

            int result = left.Prerelease.IsEmpty.CompareTo(right.Prerelease.IsEmpty);
            if (result != 0) { return result; }

            SemVerV2BnfParser leftParser = new SemVerV2BnfParser(left.Prerelease);
            SemVerV2BnfParser rightParser = new SemVerV2BnfParser(right.Prerelease);

            while (!leftParser.IsEof || !rightParser.IsEof)
            {
                // If we got to this point, both strings had a prerelease component, and
                // we are processing them. In this case, if one string runs out of prerelease
                // identifiers before the other, then the component with the fewest number
                // of prerelease identifiers sorts first.

                result = rightParser.IsEof.CompareTo(leftParser.IsEof);
                if (result != 0) { return result; }

                ReadOnlySpan<char> leftPrereleaseRemaining = leftParser.Remaining;
                ReadOnlySpan<char> rightPrereleaseRemaining = rightParser.Remaining;

                bool leftPrereleaseIdentifierIsAlphanumeric = leftParser.TryConsumeAlphanumericIdentifier();
                bool rightPrereleaseIdentifierIsAlphanumeric = rightParser.TryConsumeAlphanumericIdentifier();

                if (leftPrereleaseIdentifierIsAlphanumeric && rightPrereleaseIdentifierIsAlphanumeric)
                {
                    // Both identifiers are alphanumeric; perform an ordinal comparison.
                    // The spec uses the term "lexical" to mean ASCII symbols, not the colloquial term of "dictionary order".
                    // More info: https://github.com/semver/semver/issues/176

                    result = leftPrereleaseRemaining[..^leftParser.Remaining.Length].CompareTo(rightPrereleaseRemaining[..^rightParser.Remaining.Length], StringComparison.Ordinal);
                    if (result != 0) { return result; }
                }
                else if (leftPrereleaseIdentifierIsAlphanumeric || rightPrereleaseIdentifierIsAlphanumeric)
                {
                    // One alphanumeric identifer and one numeric identifier. Numeric always sorts first.

                    return leftPrereleaseIdentifierIsAlphanumeric.CompareTo(rightPrereleaseIdentifierIsAlphanumeric);
                }
                else
                {
                    // Both identifiers are numeric.

                    bool leftPrereleaseIdentifierIsNumeric = leftParser.TryConsumeNumericIdentifier();
                    Debug.Assert(leftPrereleaseIdentifierIsNumeric);

                    bool rightPrereleaseIdentifierIsNumeric = rightParser.TryConsumeNumericIdentifier();
                    Debug.Assert(rightPrereleaseIdentifierIsNumeric);

                    result = CompareNumericIdentifiers(leftPrereleaseRemaining[..^leftParser.Remaining.Length], rightPrereleaseRemaining[..^rightParser.Remaining.Length]);
                    if (result != 0) { return result; }
                }

                leftParser.TryConsumeLiteralChar('.'); // optional; won't exist if we're at EOF
                rightParser.TryConsumeLiteralChar('.'); // optional; won't exist if we're at EOF
            }

            // If we got to this point, the prerelease strings were equal.

            return 0;
        }

        internal static int CompareBuild(SemanticVersion left, SemanticVersion right)
        {
            // Technically the SemVer spec says build doesn't contribute to ordering,
            // but if we're being asked to compare them, we'll simplify and use Ordinal
            // ordering.

            return left.Build.CompareTo(right.Build, StringComparison.Ordinal);
        }

        private static int CompareNumericIdentifiers(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        {
            // prereq: input is well-formed
            //
            // Since identifiers don't have leading zeroes, we should compare lengths first to
            // ensure numbers aren't the same magnitude. If they're the same, then perform
            // a normal ordinal comparison.

            int result = a.Length.CompareTo(b.Length);
            if (result != 0) { return result; }

            return a.CompareTo(b, StringComparison.Ordinal);
        }
    }
}
