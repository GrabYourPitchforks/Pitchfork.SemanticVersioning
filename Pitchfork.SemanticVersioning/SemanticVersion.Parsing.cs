using System;
using System.Diagnostics.CodeAnalysis;
using Pitchfork.SemanticVersioning.Resources;

namespace Pitchfork.SemanticVersioning
{
    public sealed partial class SemanticVersion
#if NET8_0_OR_GREATER
        : ISpanParsable<SemanticVersion>
#endif
    {
        /// <summary>
        /// Parses a SemVer string into its constituent components.
        /// </summary>
        /// <param name="value">The SemVer string to parse.</param>
        /// <returns>A <see cref="Components"/> instance which represents the SemVer string's components.</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a well-formed SemVer string.</exception>
        /// <seealso cref="TryParseIntoComponents"/>
        /// <seealso cref="GetComponents"/>
        public static Components ParseIntoComponents(ReadOnlySpan<char> value)
        {
            if (!TryParseIntoComponents(value, out Components result))
            {
                throw new ArgumentException(message: SR.SemanticVersion_InvalidArgument, paramName: nameof(value));
            }

            return result;
        }

        /// <summary>
        /// Attempts to parse the provided string as a SemVer string.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="result">If this method returns <see langword="true"/>, contains
        /// the newly created <see cref="SemanticVersion"/> which represents the SemVer
        /// string. If this method returns <see langword="false"/>, contains <see langword="null"/>.</param>
        /// <returns>A Boolean value stating whether the input string successfully parsed as a well-formed
        /// SemVer string.</returns>
        /// <remarks>
        /// This method does not throw an exception on failure.
        /// </remarks>
        public static bool TryParse(string? value, [NotNullWhen(true)] out SemanticVersion? result)
        {
            if (TryParseIntoComponents(value.AsSpan(), out Components components))
            {
                result = new SemanticVersion(value!, components);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse the provided span as a SemVer string.
        /// </summary>
        /// <param name="value">The span to parse.</param>
        /// <param name="result">If this method returns <see langword="true"/>, contains
        /// the newly created <see cref="SemanticVersion"/> which represents the SemVer
        /// string. If this method returns <see langword="false"/>, contains <see langword="null"/>.</param>
        /// <returns>A Boolean value stating whether the input span successfully parsed as a well-formed
        /// SemVer string.</returns>
        /// <remarks>
        /// This method does not throw an exception on failure.
        /// </remarks>
        public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out SemanticVersion? result)
        {
            // Take the allocation here since we don't want the value to mutate out from under us, which could violate
            // our invariants, and we'll need to capture a stable copy anyway to shove it in the backing field.

            return TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// Attempts to parse a SemVer string into its constituent components.
        /// </summary>
        /// <param name="value">The SemVer string to parse, as a span.</param>
        /// <param name="result">If this method returns <see langword="true"/>, contains
        /// the parsed <see cref="Components"/> which represents the SemVer
        /// string. If this method returns <see langword="false"/>, contains <see cref="ReadOnlySpan{Char}.Empty"/>.</param>
        /// <returns>A Boolean value stating whether the input span successfully parsed as a well-formed
        /// SemVer string.</returns>
        /// <remarks>
        /// This method does not throw an exception on failure.
        /// </remarks>
        /// <seealso cref="ParseIntoComponents"/>
        /// <seealso cref="GetComponents"/>
        public static bool TryParseIntoComponents(ReadOnlySpan<char> value, out Components result) => Components.TryParse(value, out result);

#if NET8_0_OR_GREATER
        /// <inheritdoc/>
        static SemanticVersion ISpanParsable<SemanticVersion>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, out SemanticVersion? result))
            {
                throw new ArgumentException(message: SR.SemanticVersion_InvalidArgument, paramName: nameof(s));
            }

            return result;
        }

        /// <inheritdoc/>
        static bool ISpanParsable<SemanticVersion>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [NotNullWhen(true)] out SemanticVersion? result) => TryParse(s, out result);

        /// <inheritdoc/>
        static SemanticVersion IParsable<SemanticVersion>.Parse(string s, IFormatProvider? provider)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (!TryParse(s, out SemanticVersion? result))
            {
                throw new ArgumentException(message: SR.SemanticVersion_InvalidArgument, paramName: nameof(s));
            }

            return result;
        }

        /// <inheritdoc/>
        static bool IParsable<SemanticVersion>.TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out SemanticVersion? result) => TryParse(s, out result);
#endif
    }
}
