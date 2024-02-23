using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Pitchfork.SemanticVersioning.Tests
{
    public static class SemanticVersionEquivalenceComparerTests
    {
        [Fact]
        public static void Class_HasNoPublicInstanceCtor()
        {
            var allPublicInstanceCtors = typeof(SemanticVersionEquivalenceComparer).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Assert.Empty(allPublicInstanceCtors);
        }

        [Fact]
        public static void InstanceProperty_ReturnsSingletonInstance()
        {
            var instance1 = SemanticVersionEquivalenceComparer.Instance;
            var instance2 = SemanticVersionEquivalenceComparer.Instance;

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public static void TestComparisonsAndEquality_OverOrderedDistinctData()
        {
            List<SemanticVersion> orderedTestItems = OrderedDistinctTestData().Select(x => new SemanticVersion(x)).ToList();
            orderedTestItems.Insert(0, null); // null always comes before everything else

            for (int i = 0; i < orderedTestItems.Count; i++)
            {
                for (int j = 0; j < orderedTestItems.Count; j++)
                {
                    var left = orderedTestItems[i];
                    var right = orderedTestItems[j];
                    int expectedSign = Math.Sign(i.CompareTo(j));

                    try
                    {
                        _TestComparisonRunner(left, right, expectedSign);
                    }
                    catch (Exception ex)
                    {
                        throw new XunitException($"Failed comparing {left ?? (object)"<null>"} to {right ?? (object)"<null>"} (expected sign = {expectedSign})", ex);
                    }
                }
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("1.2.3", "1.2.3")]
        [InlineData("1.2.3-preview", "1.2.3-preview")]
        [InlineData("2.3.4-abc+build", "2.3.4-abc+build")] // equal builds, build metadata is ignored
        [InlineData("2.3.4-abc+build1", "2.3.4-abc+build2")] // different builds, build metadata is ignored
        [InlineData("2.3.4-abc+build", "2.3.4-abc")] // one with build metadata, one without, metadata is ignored
        [InlineData("6.7.8+build1", "6.7.8+build2")] // different builds, build metadata is ignored
        [InlineData("6.7.9", "6.7.9+mybuild")] // build metadata ignored
        public static void TestComparisonsAndEquality_OverEquivalentData(string x, string y)
        {
            var left = (x is not null) ? new SemanticVersion(x) : null;
            var right = (y is not null) ? new SemanticVersion(y) : null;
            _TestComparisonRunner(left, right, expectedSign: 0);
        }

        private static void _TestComparisonRunner(SemanticVersion left, SemanticVersion right, int expectedSign)
        {
            var comparer = SemanticVersionEquivalenceComparer.Instance;

            // IEqualityComparer<T> implementation

            if (expectedSign == 0)
            {
                Assert.True(comparer.Equals(left, right));
                Assert.Equal(comparer.GetHashCode(left), comparer.GetHashCode(right));
            }
            else
            {
                Assert.False(comparer.Equals(left, right));
                // n.b. GetHashCode not required to be different for unequal objects
            }

            // IComparable<T> implementation

            Assert.Equal(expectedSign, Math.Sign(comparer.Compare(left, right)));
        }

        private static IEnumerable<string> OrderedDistinctTestData()
        {
            // It's a subset of cases from CommonTestData.
            // Allow the normal SemVer comparison routine to contain the full test battery.

            yield return "1.0.0-alpha"; // prerelease before release
            yield return "1.0.0";
            yield return "1.0.8-alpha"; // prerelease before release, alphabetical ordering
            yield return "1.0.8-beta.0"; // numeric ordering within prerelease
            yield return "1.0.8-beta.5";
            yield return "1.0.8-beta.10";
            yield return "1.0.8-beta.20.6000"; // numeric before alphabetical
            yield return "1.0.8-beta.20.a.bc";
            yield return "1.0.8-beta.20.def";
            yield return "1.0.8";
            yield return "2.0.0-PREVIEW"; // uppercase before lowercase
            yield return "2.0.0-preview";
            yield return "2.0.0";
            yield return "2.0.5";
            yield return "2.3.0-preview";
            yield return "2.3.0";
            yield return "2.3.1-preview";
            yield return "2.3.1";
            yield return "2.10.1-preview";
            yield return "2.10.1";
        }
    }
}
