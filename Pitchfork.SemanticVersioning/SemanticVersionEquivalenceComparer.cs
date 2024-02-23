using System;
using System.Collections.Generic;

namespace Pitchfork.SemanticVersioning
{
    /// <summary>
    /// A comparer which disregards a <see cref="SemanticVersion"/>'s build metadata for the purpose of comparison.
    /// Two <see cref="SemanticVersion"/> instances which differ only in their build metadata will be considered equal.
    /// </summary>
    public sealed class SemanticVersionEquivalenceComparer : IComparer<SemanticVersion>, IEqualityComparer<SemanticVersion>
    {
        private SemanticVersionEquivalenceComparer() { /* singleton ctor */ }

        /// <summary>
        /// Retrieves the static singleton <see cref="SemanticVersionEquivalenceComparer"/> instance.
        /// </summary>
        /// <remarks>
        /// The returned instance is thread-safe for all uses.
        /// </remarks>
        public static SemanticVersionEquivalenceComparer Instance { get; } = new SemanticVersionEquivalenceComparer();

        /// <summary>
        /// Compares two <see cref="SemanticVersion"/> instances for equivalence, disregarding build metadata,
        /// and returns a value indicating their relative ordering.
        /// </summary>
        /// <param name="x">The left instance to compare.</param>
        /// <param name="y">The right instance to compare.</param>
        /// <returns>A negative value if <paramref name="x"/> is ordered before <paramref name="y"/>;
        /// zero if <paramref name="x"/> and <paramref name="y"/> have the same ordering; and
        /// a positive value if <paramref name="x"/> is ordered after <paramref name="y"/>.</returns>
        /// <remarks>
        /// Two null instances will always compare as equal. A null instance will have a lower
        /// ordering when compared against a non-null instance.
        /// </remarks>
        public int Compare(SemanticVersion? x, SemanticVersion? y)
        {
            if (x is null || y is null)
            {
                return (y is null).CompareTo(x is null); // null sorts before non-null
            }

            int result;

            // Major.Minor.Patch

            result = SemVerV2ComparisonUtil.CompareMajorMinorPatch(x, y);
            if (result != 0) { return result; }

            // Prerelease (optional!)
            // Build is not considered.

            return SemVerV2ComparisonUtil.ComparePrerelease(x, y);
        }

        /// <summary>
        /// Compares two <see cref="SemanticVersion"/> instances for equivalence, disregarding build metadata,
        /// and returns a value indicating whether the instances are equal.
        /// </summary>
        /// <param name="x">The left instance to compare.</param>
        /// <param name="y">The right instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="x"/> and <paramref name="y"/> are equivalent;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Equals(SemanticVersion? x, SemanticVersion? y)
        {
            if (ReferenceEquals(x, y)) { return true; }
            if (x is null || y is null) { return false; } // null != non-null

            return x.Major.Equals(y.Major, StringComparison.Ordinal)
                && x.Minor.Equals(y.Minor, StringComparison.Ordinal)
                && x.Patch.Equals(y.Patch, StringComparison.Ordinal)
                && x.Prerelease.Equals(y.Prerelease, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="SemanticVersion"/> instance.
        /// </summary>
        /// <param name="obj">The instance for which to compute the hash code.</param>
        /// <returns>A hash code for the specified instance.</returns>
        /// <remarks>
        /// The returned hash code is specific to how <see cref="SemanticVersionEquivalenceComparer"/>
        /// performs equivalence checks for <see cref="SemanticVersion"/> objects. It is not
        /// guaranteed to have the same return value as <see cref="SemanticVersion.GetHashCode"/>,
        /// since <see cref="SemanticVersion"/>'s built-in equality checks consider build metadata.
        /// </remarks>
        public int GetHashCode(SemanticVersion? obj)
        {
            if (obj is null) { return 0; }

            ReadOnlySpan<char> spanToHash = obj.ToString().AsSpan();
            if (!obj.Build.IsEmpty)
            {
                spanToHash = spanToHash[..^(obj.Build.Length + 1)]; // remove build metadata and preceding plus
            }

            // The "XOR 1" below is to prevent callers from inadvertently relying on the hash codes
            // from this comparer being equivalent to the hash codes from SemanticVersion.GetHashCode,
            // since we have different comparison and equality semantics.

#if NETCOREAPP
            // on netcoreapp, string.GetHashCode is already collision-resistant
            return string.GetHashCode(spanToHash) ^ 1;
#else
            // on other platforms, we'll use xxHash as a collision resistant mechanism
            return StringHasher.ComputeHash(spanToHash) ^ 1;
#endif
        }
    }
}
