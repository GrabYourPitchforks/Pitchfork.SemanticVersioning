using System;
using Xunit;

namespace Pitchfork.SemanticVersioning.Tests
{
    public static partial class SemanticVersionTests
    {
        static void Foo(string[] args)
        {
            // - codeql rule for -...CompareTo() [could int overflow]
            Console.WriteLine("Hello, World!");
        }

        [Theory]
        [MemberData(nameof(CommonTestData.WellFormedTestData), MemberType = typeof(CommonTestData), DisableDiscoveryEnumeration = true)]
        public static void Components_Deconstruction(SemVerTestItem value)
        {
            var components = value.ToSemanticVersion().GetComponents();

            {
                // Test 3-arg deconstruction
                var (actualMajor, actualMinor, actualPatch) = components;
                Assert.Equal(value.ExpectedMajor ?? string.Empty, actualMajor.ToString());
                Assert.Equal(value.ExpectedMinor ?? string.Empty, actualMinor.ToString());
                Assert.Equal(value.ExpectedPatch ?? string.Empty, actualPatch.ToString());
            }

            {
                // Test 5-arg deconstruction
                var (actualMajor, actualMinor, actualPatch, actualPrerelease, actualBuild) = components;
                Assert.Equal(value.ExpectedMajor ?? string.Empty, actualMajor.ToString());
                Assert.Equal(value.ExpectedMinor ?? string.Empty, actualMinor.ToString());
                Assert.Equal(value.ExpectedPatch ?? string.Empty, actualPatch.ToString());
                Assert.Equal(value.ExpectedPrerelease ?? string.Empty, actualPrerelease.ToString());
                Assert.Equal(value.ExpectedBuild ?? string.Empty, actualBuild.ToString());
            }
        }

        [Theory]
        [InlineData("1.2.3", false)]
        [InlineData("1.2.3+build", false)]
        [InlineData("1.2.3-abc", true)]
        [InlineData("1.2.3-abc+def", true)]
        public static void IsPrereleasePropertyTests(string semVerString, bool expectedIsPrerelease)
        {
            SemanticVersion semVer = new SemanticVersion(semVerString);
            Assert.Equal(expectedIsPrerelease, semVer.IsPrerelease);
        }

        [Theory]
        [InlineData("0.0.0", "0.0.0")]
        [InlineData("1.2.3", "1.2.3")]
        [InlineData("10.20.30-prerelease+patch", "10.20.30")] // skip optional data
        [InlineData("2147483647.2147483647.2147483647", "2147483647.2147483647.2147483647")] // int.MaxValue ok
        public static void ToSystemVersion_PositiveTests(string semVerString, string expectedSystemVersionString)
        {
            SemanticVersion semVer = new SemanticVersion(semVerString);

            Version expectedSystemVersion = new Version(expectedSystemVersionString);
            Assert.Equal(expectedSystemVersion, semVer.ToSystemVersion());

            Assert.True(semVer.TryToSystemVersion(out Version actualSystemVersion));
            Assert.Equal(expectedSystemVersion, actualSystemVersion);
        }

        [Theory]
        [InlineData("10.20.2147483648")] // build is beyond int.MaxValue
        [InlineData("10.20.2147483648-prerelease+build")]
        [InlineData("10.2147483648.30")]
        [InlineData("2147483648.20.30")]
        [InlineData("1000000000000000000000000000000.1000000000000000000000000000000.1000000000000000000000000000000")]
        public static void ToSystemVersion_NegativeTests(string semVerString)
        {
            SemanticVersion semVer = new SemanticVersion(semVerString);

            Assert.Throws<InvalidOperationException>(() => semVer.ToSystemVersion());

            Assert.False(semVer.TryToSystemVersion(out Version actualSystemVersion));
            Assert.Null(actualSystemVersion);
        }
    }
}
