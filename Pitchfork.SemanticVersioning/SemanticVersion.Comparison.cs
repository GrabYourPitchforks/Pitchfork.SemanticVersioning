using System;
using System.Numerics;
using Pitchfork.SemanticVersioning.Resources;

namespace Pitchfork.SemanticVersioning
{
    public sealed partial class SemanticVersion : IComparable, IComparable<SemanticVersion?>, IEquatable<SemanticVersion?>
#if NET8_0_OR_GREATER
        , IComparisonOperators<SemanticVersion, SemanticVersion, bool>
#endif
    {
        /// <inheritdoc/>
        public static bool operator ==(SemanticVersion? left, SemanticVersion? right)
            => left is null ? right is null : left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(SemanticVersion? left, SemanticVersion? right) => !(left == right);

        /// <inheritdoc/>
        public static bool operator <(SemanticVersion? left, SemanticVersion? right)
            => left is null ? right is not null : ((IComparable<SemanticVersion?>)left).CompareTo(right) < 0;

        /// <inheritdoc/>
        public static bool operator <=(SemanticVersion? left, SemanticVersion? right) => !(right < left);

        /// <inheritdoc/>
        public static bool operator >(SemanticVersion? left, SemanticVersion? right) => (right < left);

        /// <inheritdoc/>
        public static bool operator >=(SemanticVersion? left, SemanticVersion? right) => !(left < right);

        /// <inheritdoc cref="IComparable{SemanticVersion}.CompareTo"/>
        public int CompareTo(SemanticVersion? other)
        {
            if (other is null) { return 1; } // null always sorts before non-null

            int result;

            // Major.Minor.Patch

            result = SemVerV2ComparisonUtil.CompareMajorMinorPatch(this, other);
            if (result != 0) { return result; }

            // Prerelease (optional!)

            result = SemVerV2ComparisonUtil.ComparePrerelease(this, other);
            if (result != 0) { return result; }

            // Build (optional!)
            // Technically the SemVer spec says build doesn't contribute to ordering,
            // but we need to consider it since the build string factors in to our
            // Equals method's return value.

            return SemVerV2ComparisonUtil.CompareBuild(this, other);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as SemanticVersion);

        /// <summary>
        /// Determines whether the specified <see cref="SemanticVersion"/> is equal
        /// to the current <see cref="SemanticVersion"/>.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns><see langword="true"/> if the two objects are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(SemanticVersion? other) => other is not null && this._versionString == other._versionString;

        /// <summary>
        /// Retrieves the major, minor, patch, prerelease, and build components of the current <see cref="SemanticVersion"/>.
        /// </summary>
        /// <returns>A <see cref="Components"/> which contains the deconstructed form of this <see cref="SemanticVersion"/>.</returns>
        /// <seealso cref="ParseIntoComponents"/>
        /// <seealso cref="TryParseIntoComponents"/>
        public Components GetComponents() => new Components(this);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Keep this in sync with SemanticVerisonEquivalenceComparer.

#if NETCOREAPP
            // on netcoreapp, string.GetHashCode is already collision-resistant
            return _versionString.GetHashCode();
#else
            // on other platforms, we'll use xxHash as a collision resistant mechanism
            return StringHasher.ComputeHash(_versionString);
#endif
        }

        /// <inheritdoc/>
        int IComparable.CompareTo(object? obj)
        {
            if (obj is null) { return 1; } // null always sorts before non-null

            if (obj is not SemanticVersion other)
            {
                throw new ArgumentException(
                    message: string.Format(SR.Common_ArgIsWrongType, obj.GetType(), typeof(SemanticVersion)),
                    paramName: nameof(obj));
            }

            return ((IComparable<SemanticVersion>)this).CompareTo(other);
        }
    }
}
