using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Pitchfork.SemanticVersioning.Resources;

namespace Pitchfork.SemanticVersioning
{
    /// <summary>
    /// Represents a v2.0.0 Semantic Version string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="https://semver.org/">semver.org</a> for more information.
    /// </para>
    /// <para>
    /// For the purposes of ordering and equality, if two <see cref="SemanticVersion"/> instances
    /// differ only in their <see cref="Build"/> values, the instances are considered not equal
    /// and will be ordered using a case-sensitive ordinal comparison over the build metadata.
    /// For example, <em>1.2.3+aaa</em> will order before <em>1.2.3+bbb</em>, and <em>1.2.3+10</em>
    /// will order before <em>1.2.3+8</em>.
    /// </para>
    /// </remarks>
    public sealed partial class SemanticVersion
    {
        private readonly string _versionString;
        private readonly int _minorStartIdx; // does not include leading dot; always > 0
        private readonly int _patchStartIdx; // does not include leading dot; always > 0
        private readonly int _prereleaseLength = -1; // does not include leading hyphen; -1 represents no prerelease metadata
        private readonly int _buildLength = -1; // does not include leading plus; -1 represents no build metadata

        /// <summary>
        /// Constructs a new <see cref="SemanticVersion"/> instance from an existing
        /// <see cref="Version"/> object, with optional prerelease and build metadata.
        /// </summary>
        /// <param name="version">The <see cref="Version"/> object to use as the core SemVer version.</param>
        /// <param name="prerelease">An optional prerelease metadata argument, not including the leading hyphen.</param>
        /// <param name="build">An optional build metadata argument, not including the leading plus.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="version"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="version"/> has an explicit non-zero revision number; or
        /// <paramref name="prerelease"/> is non-empty but is not a valid prerelease metadata string; or
        /// <paramref name="build"/> is non-empty but is not a valid build metadata string.
        /// </exception>
        public SemanticVersion(Version version, string? prerelease = null, string? build = null)
        {
            if (version is null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (version.Revision != -1 && version.Revision != 0)
            {
                throw new ArgumentException(
                    message: SR.SemanticVersion_VersionCannotHaveNonZeroRevision,
                    paramName: nameof(version));
            }

            // n.b. the input to the ctor takes prerelease and build are strings instead of ROS<char>
            // because we can't allow them to mutate out from under us between inspection and when
            // we use them to build the final version string.

            ValidatePrereleaseAndBuildWellFormedness(prerelease, build);

            // string concat isn't the end of the world considering how rare it's expected to be

            _versionString = $"{version.Major}.{version.Minor}.{Math.Max(0, version.Build)}";
            _minorStartIdx = _versionString.IndexOf('.') + 1;
            _patchStartIdx = _versionString.IndexOf('.', _minorStartIdx + 1) + 1;

            if (!string.IsNullOrEmpty(prerelease))
            {
                _prereleaseLength = prerelease.Length;
                _versionString += $"-{prerelease}";
            }

            if (!string.IsNullOrEmpty(build))
            {
                _buildLength = build.Length;
                _versionString += $"+{build}";
            }
        }

        /// <summary>
        /// Constructs a new <see cref="SemanticVersion"/> instance from an existing
        /// <see cref="SemanticVersion"/> object, with optional prerelease and build metadata.
        /// The original <see cref="SemanticVersion"/> object's prerelease and build metadata
        /// are not copied into the new object.
        /// </summary>
        /// <param name="baseVersion">The <see cref="SemanticVersion"/> object to use as the core SemVer version.</param>
        /// <param name="prerelease">An optional prerelease metadata argument, not including the leading hyphen.</param>
        /// <param name="build">An optional build metadata argument, not including the leading plus.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="baseVersion"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="prerelease"/> is non-empty but is not a valid prerelease metadata string; or
        /// <paramref name="build"/> is non-empty but is not a valid build metadata string.
        /// </exception>
        /// <remarks>
        /// <paramref name="baseVersion"/>'s prerelease and build metadata are not copied into the
        /// newly constructed object. If the newly constructed object should have associated
        /// prerelease or build metadata, that data must be explicitly provided via the
        /// <paramref name="prerelease"/> and <paramref name="build"/> arguments.
        /// </remarks>
        public SemanticVersion(SemanticVersion baseVersion, string? prerelease, string? build)
        {
            if (baseVersion is null)
            {
                throw new ArgumentNullException(nameof(baseVersion));
            }

            // n.b. the input to the ctor takes prerelease and build are strings instead of ROS<char>
            // because we can't allow them to mutate out from under us between inspection and when
            // we use them to build the final version string.

            ValidatePrereleaseAndBuildWellFormedness(prerelease, build);

            // string concat isn't the end of the world considering how rare it's expected to be

            _versionString = baseVersion._versionString[..^(baseVersion._prereleaseLength + baseVersion._buildLength + 2)]; // strip existing prerelease and build metadata
            _minorStartIdx = baseVersion._minorStartIdx;
            _patchStartIdx = baseVersion._patchStartIdx;

            if (!string.IsNullOrEmpty(prerelease))
            {
                _prereleaseLength = prerelease.Length;
                _versionString += $"-{prerelease}";
            }

            if (!string.IsNullOrEmpty(build))
            {
                _buildLength = build.Length;
                _versionString += $"+{build}";
            }
        }

        /// <summary>
        /// Constructs a new <see cref="SemanticVersion"/> instance from a SemVer string.
        /// </summary>
        /// <param name="value">The SemVer string to parse.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is not a well-formed SemVer string.
        /// </exception>
        /// <remarks>
        /// <paramref name="value"/> must be well-formed per the SemVer v2.0.0 specification. Whitespace
        /// characters are not allowed.
        /// </remarks>
        public SemanticVersion(string value)
            : this(value ?? throw new ArgumentNullException(nameof(value)), ParseIntoComponents(value.AsSpan()))
        {
        }

        public SemanticVersion(int major, int minor, int patch, string? prerelease = null, string? build = null)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major));
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor));
            }

            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch));
            }

            // n.b. the input to the ctor takes prerelease and build are strings instead of ROS<char>
            // because we can't allow them to mutate out from under us between inspection and when
            // we use them to build the final version string.

            ValidatePrereleaseAndBuildWellFormedness(prerelease, build);

            StringBuilder builder = new StringBuilder();
            builder.Append(major);
            builder.Append('.');

            _minorStartIdx = builder.Length;
            builder.Append(minor);
            builder.Append('.');

            _patchStartIdx = builder.Length;
            builder.Append(patch);

            if (!string.IsNullOrEmpty(prerelease))
            {
                _prereleaseLength = prerelease.Length;
                builder.Append('-');
                builder.Append(prerelease);
            }

            if (!string.IsNullOrEmpty(build))
            {
                _buildLength = build.Length;
                builder.Append('+');
                builder.Append(build);
            }

            _versionString = builder.ToString();
        }

        // no arg checks
        private SemanticVersion(string value, Components components)
        {
            Debug.Assert(value.AsSpan().Overlaps(components.Major), "Provided input doesn't match captured spans.");

            _versionString = value;
            _minorStartIdx = components.Major.Length + 1;
            _patchStartIdx = _minorStartIdx + components.Minor.Length + 1;
            if (!components.Prerelease.IsEmpty)
            {
                _prereleaseLength = components.Prerelease.Length;
            }
            if (!components.Build.IsEmpty)
            {
                _buildLength = components.Build.Length;
            }
        }

        /// <summary>
        /// Gets the major version string from this instance. For example, given a
        /// SemVer string of <em>1.2.3-preview.4+AABBCC</em>, this property returns
        /// the value <em>1</em>.
        /// </summary>
        /// <remarks>
        /// This value is guaranteed to represent a non-negative integer, but the
        /// SemVer specification does not guarantee that the value fits into an
        /// <see cref="int"/> or any other finite-length data type.
        /// </remarks>
        public ReadOnlySpan<char> Major => _versionString.AsSpan()[..(_minorStartIdx - 1)];

        /// <summary>
        /// Gets the minor version string from this instance. For example, given a
        /// SemVer string of <em>1.2.3-preview.4+AABBCC</em>, this property returns
        /// the value <em>2</em>.
        /// </summary>
        /// <remarks>
        /// This value is guaranteed to represent a non-negative integer, but the
        /// SemVer specification does not guarantee that the value fits into an
        /// <see cref="int"/> or any other finite-length data type.
        /// </remarks>
        public ReadOnlySpan<char> Minor => _versionString.AsSpan()[_minorStartIdx..(_patchStartIdx - 1)];

        /// <summary>
        /// Gets the patch version string from this instance. For example, given a
        /// SemVer string of <em>1.2.3-preview.4+AABBCC</em>, this property returns
        /// the value <em>3</em>.
        /// </summary>
        /// <remarks>
        /// This value is guaranteed to represent a non-negative integer, but the
        /// SemVer specification does not guarantee that the value fits into an
        /// <see cref="int"/> or any other finite-length data type.
        /// </remarks>
        public ReadOnlySpan<char> Patch => _versionString.AsSpan()[_patchStartIdx..^(_prereleaseLength + 1 + _buildLength + 1)];

        /// <summary>
        /// Gets the prerelease metadata from this instance, if it exists. For example,
        /// given a SemVer string of <em>1.2.3-preview.4+AABBCC</em>, this property returns
        /// the value <em>preview.4</em>.
        /// </summary>
        /// <remarks>
        /// If there is no prerelease metadata associated with this instance, this property returns an empty span.
        /// </remarks>
        public ReadOnlySpan<char> Prerelease => (_prereleaseLength >= 0) ? _versionString.AsSpan()[^(_prereleaseLength + _buildLength + 1)..^(_buildLength + 1)] : ReadOnlySpan<char>.Empty;

        /// <summary>
        /// Gets the build metadata from this instance, if it exists. For example,
        /// given a SemVer string of <em>1.2.3-preview.4+AABBCC</em>, this property returns
        /// the value <em>AABBCC</em>.
        /// </summary>
        /// <remarks>
        /// If there is no build metadata associated with this instance, this property returns an empty span.
        /// </remarks>
        public ReadOnlySpan<char> Build => (_buildLength >= 0) ? _versionString.AsSpan()[^_buildLength..] : ReadOnlySpan<char>.Empty;

        /// <summary>
        /// Returns a value stating whether this instance represents a prerelease version.
        /// </summary>
        public bool IsPrerelease => _prereleaseLength > 0;

        /// <summary>
        /// Returns the SemVer 2.0.0 compatible string representation of this instance.
        /// </summary>
        /// <returns>This instance as a SemVer 2.0.0 string.</returns>
        public override string ToString() => _versionString;

        /// <summary>
        /// Returns a <see cref="Version"/> that represents the <em>Major.Minor.Patch</em> version
        /// of this instance. Prerelease and build metadata are ignored.
        /// </summary>
        /// <returns>The <see cref="Version"/> equivalent of the core version of this instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="Major"/>, <see cref="Minor"/>, or <see cref="Patch"/> is outside the range of
        /// legal <see cref="int"/> values.
        /// </exception>
        /// <remarks>
        /// For example, assuming the current instance represents the SemVer string <em>1.2.3-preview.4+AABBCC</em>,
        /// the newly created <see cref="Version"/> instance will have the value <em>1.2.3</em>, and the
        /// <see cref="Version.Revision"/> property will have the value <em>-1</em>.
        /// </remarks>
        /// <seealso cref="IsCoreVersion"/>
        /// <seealso cref="TryToSystemVersion"/>
        public Version ToSystemVersion()
        {
            if (TryToSystemVersion(out Version? result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException(SR.SemanticVersion_CannotConvertToSystemVersion);
            }
        }

        /// <summary>
        /// Attempts to create a <see cref="Version"/> that represents the <em>Major.Minor.Patch</em> version
        /// of this instance. Prerelease and build metadata are ignored.
        /// </summary>
        /// <param name="result">If this method returns <see langword="true"/>, contains the <see cref="Version"/>
        /// which represents the core version of this instance. If this method returns <see langword="false"/>,
        /// contains <see langword="null"/>.</param>
        /// <returns>A Boolean value stating whether a <see cref="Version"/> equivalent of the
        /// core version of this instance could be created.</returns>
        /// <remarks>
        /// <para>
        /// For example, assuming the current instance represents the SemVer string <em>1.2.3-preview.4+AABBCC</em>,
        /// the newly created <see cref="Version"/> instance will have the value <em>1.2.3</em>, and the
        /// <see cref="Version.Revision"/> property will have the value <em>-1</em>.
        /// </para>
        /// <para>
        /// This method returns <see langword="false"/> if <see cref="Major"/>, <see cref="Minor"/>, or
        /// <see cref="Patch"/> is outside the range of legal <see cref="int"/> values.
        /// This method does not throw an exception on failure.
        /// </para>
        /// </remarks>
        /// <seealso cref="IsCoreVersion"/>
        /// <seealso cref="ToSystemVersion"/>
        public bool TryToSystemVersion([NotNullWhen(true)] out Version? result)
        {
            if (TryParseKnownGoodDecimalInt(Major, out int major)
                && TryParseKnownGoodDecimalInt(Minor, out int minor)
                && TryParseKnownGoodDecimalInt(Patch, out int patch))
            {
                result = new Version(major, minor, patch);
                return true;
            }
            else
            {
                result = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool TryParseKnownGoodDecimalInt(ReadOnlySpan<char> value, out int result)
            {
                // Prerequisite: value is non-empty, has no leading zeroes, and is composed
                // solely of the chars [0-9] (no nulls, no whitespace, no non-ASCII decimal digits).

#if NET6_0_OR_GREATER
                return int.TryParse(value, NumberStyles.None, null, out result); // this is invariant in netcore
#else
                // values greater than 10 chars can't possibly fit into an int32
                if (value.Length <= 10)
                {
                    int tempResult = 0;
                    foreach (char ch in value)
                    {
                        Debug.Assert(ch is >= '0' and <= '9', "Non-decimal digit encountered.");

                        // We're going to use the sign bit to detect overflow. We can't allow the
                        // multiplication to overflow since it won't reliably set the sign bit, so
                        // we'll put an explicit guard there. The addition *will* reliably set the
                        // sign bit on overflow, so we don't need a guard in front of it.

                        if (tempResult > int.MaxValue / 10) { goto Overflow; }
                        tempResult = unchecked(tempResult * 10 + ch - '0');
                        if (tempResult < 0) { goto Overflow; }
                    }

                    result = tempResult;
                    return true;
                }

            Overflow:
                result = default;
                return false;
#endif
            }
        }

        private static void ValidatePrereleaseAndBuildWellFormedness(string? prerelease, string? build)
        {
            if (!string.IsNullOrEmpty(prerelease))
            {
                SemVerV2BnfParser parser = new SemVerV2BnfParser(prerelease.AsSpan());
                if (!parser.TryConsumePrerelease() || !parser.IsEof) // order matters since evaluation has side effects
                {
                    throw new ArgumentException(
                        message: SR.SemanticVersion_InvalidArgument,
                        paramName: nameof(prerelease));
                }
            }

            if (!string.IsNullOrEmpty(build))
            {
                SemVerV2BnfParser parser = new SemVerV2BnfParser(build.AsSpan());
                if (!parser.TryConsumeBuild() || !parser.IsEof) // order matters since evaluation has side effects
                {
                    throw new ArgumentException(
                        message: SR.SemanticVersion_InvalidArgument,
                        paramName: nameof(build));
                }
            }
        }
    }
}
