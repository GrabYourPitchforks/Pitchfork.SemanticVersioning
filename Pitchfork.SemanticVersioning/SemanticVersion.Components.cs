using System;
using System.ComponentModel;

namespace Pitchfork.SemanticVersioning
{
    public sealed partial class SemanticVersion
    {
        /// <summary>
        /// Represents the constituent components of a SemVer string.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ParseIntoComponents"/> or <see cref="TryParseIntoComponents"/>
        /// to get an instance of this type.
        /// </remarks>
        public readonly ref struct Components
        {
            internal Components(SemanticVersion version)
            {
                Major = version.Major;
                Minor = version.Minor;
                Patch = version.Patch;
                Prerelease = version.Prerelease;
                Build = version.Build;
            }

            /// <summary>
            /// The major version string from this instance.
            /// </summary>
            /// <remarks>
            /// See <see cref="SemanticVersion.Major"/> for more information.
            /// </remarks>
            public ReadOnlySpan<char> Major { get; private init; }

            /// <summary>
            /// The minor version string from this instance.
            /// </summary>
            /// <remarks>
            /// See <see cref="SemanticVersion.Minor"/> for more information.
            /// </remarks>
            public ReadOnlySpan<char> Minor { get; private init; }

            /// <summary>
            /// The patch version string from this instance.
            /// </summary>
            /// <remarks>
            /// See <see cref="SemanticVersion.Patch"/> for more information.
            /// </remarks>
            public ReadOnlySpan<char> Patch { get; private init; }

            /// <summary>
            /// The prerelease metadata from this instance.
            /// </summary>
            /// <remarks>
            /// See <see cref="SemanticVersion.Prerelease"/> for more information.
            /// </remarks>
            public ReadOnlySpan<char> Prerelease { get; private init; }

            /// <summary>
            /// The build metadata from this instance.
            /// </summary>
            /// <remarks>
            /// See <see cref="SemanticVersion.Build"/> for more information.
            /// </remarks>
            public ReadOnlySpan<char> Build { get; private init; }

            /// <summary>
            /// Supports the C# deconstruction pattern.
            /// </summary>
            /// <remarks>
            /// This overload returns the major version string, the minor version string, and the patch version string.
            /// The prerelease and build metadata are ignored.
            /// </remarks>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out ReadOnlySpan<char> major, out ReadOnlySpan<char> minor, out ReadOnlySpan<char> patch)
            {
                major = Major;
                minor = Minor;
                patch = Patch;
            }

            /// <summary>
            /// Supports the C# deconstruction pattern.
            /// </summary>
            /// <remarks>
            /// This overload returns the major version string, the minor version string, the patch version string,
            /// the prerelease metadata (which may be empty), and the build metadata (which may be empty)
            /// </remarks>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Deconstruct(out ReadOnlySpan<char> major, out ReadOnlySpan<char> minor, out ReadOnlySpan<char> patch, out ReadOnlySpan<char> prerelease, out ReadOnlySpan<char> build)
            {
                major = Major;
                minor = Minor;
                patch = Patch;
                prerelease = Prerelease;
                build = Build;
            }

            internal static bool TryParse(ReadOnlySpan<char> originalValue, out Components result)
            {
                // <Major>.<Minor>.<Patch>[-<Prerelease>][+<Build>]

                SemVerV2BnfParser parser = new SemVerV2BnfParser(originalValue);

                if (!parser.TryConsumeNumericIdentifier()) { goto Fail; }
                ReadOnlySpan<char> major = originalValue[..^parser.Remaining.Length];

                if (!parser.TryConsumeLiteralChar('.')) { goto Fail; }
                if (!parser.TryConsumeNumericIdentifier()) { goto Fail; }
                ReadOnlySpan<char> minor = originalValue[(major.Length + 1)..^parser.Remaining.Length];

                if (!parser.TryConsumeLiteralChar('.')) { goto Fail; }
                if (!parser.TryConsumeNumericIdentifier()) { goto Fail; }
                ReadOnlySpan<char> patch = originalValue[(major.Length + minor.Length + 2)..^parser.Remaining.Length];

                ReadOnlySpan<char> prerelease = ReadOnlySpan<char>.Empty;
                if (parser.TryConsumeLiteralChar('-'))
                {
                    if (!parser.TryConsumePrerelease()) { goto Fail; }
                    prerelease = originalValue[(major.Length + minor.Length + patch.Length + 3)..^parser.Remaining.Length];
                }

                ReadOnlySpan<char> build = ReadOnlySpan<char>.Empty;
                if (parser.TryConsumeLiteralChar('+'))
                {
                    build = parser.Remaining; // assuming EOF check below passes
                    if (!parser.TryConsumeBuild()) { goto Fail; }
                }

                if (!parser.IsEof) { goto Fail; }

                result = new Components
                {
                    Major = major,
                    Minor = minor,
                    Patch = patch,
                    Prerelease = prerelease,
                    Build = build
                };
                return true;

            Fail:
                result = default;
                return false;
            }
        }
    }
}
