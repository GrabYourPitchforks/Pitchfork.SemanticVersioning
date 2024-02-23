using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pitchfork.SemanticVersioning.Tests
{
    public static partial class SemanticVersionTests
    {
        [Theory]
        [MemberData(nameof(CommonTestData.WellFormedTestData), MemberType = typeof(CommonTestData), DisableDiscoveryEnumeration = true)]
        public static void TestConstructionForWellFormedInputs(SemVerTestItem item)
        {
            // Normal ctor

            var constructedObj1 = new SemanticVersion(item.SemVerString);
            _AssertConstructedProperties(constructedObj1, item);

            // Parse routines

            Assert.True(SemanticVersion.TryParse(item.SemVerString, out var constructedObj2));
            _AssertConstructedProperties(constructedObj2, item);

            Assert.True(SemanticVersion.TryParse(item.SemVerString.AsSpan(), out var constructedObj3));
            _AssertConstructedProperties(constructedObj3, item);

            var componentsObj1 = SemanticVersion.ParseIntoComponents(item.SemVerString.AsSpan());
            _AssertComponentsProperties(componentsObj1, item, assertNoAllocation: true);

            var componentsObj2 = constructedObj1.GetComponents();
            _AssertComponentsProperties(componentsObj2, item, assertNoAllocation: false);

            Assert.True(SemanticVersion.TryParseIntoComponents(item.SemVerString.AsSpan(), out var componentsObj3));
            _AssertComponentsProperties(componentsObj2, item, assertNoAllocation: true);

#if NET8_0_OR_GREATER
            _TestIParsableStaticInterfaceMethods<SemanticVersion>(item);
            _TestISpanParsableStaticInterfaceMethods<SemanticVersion>(item);
            static void _TestIParsableStaticInterfaceMethods<T>(SemVerTestItem item) where T : IParsable<T>
            {
                var constructedObj4 = T.Parse(item.SemVerString, provider: null /* unused */);
                _AssertConstructedProperties((SemanticVersion)(object)constructedObj4, item);

                Assert.True(T.TryParse(item.SemVerString, provider: null /* unused */, out var constructedObj5));
                _AssertConstructedProperties((SemanticVersion)(object)constructedObj5, item);
            }
            static void _TestISpanParsableStaticInterfaceMethods<T>(SemVerTestItem item) where T : ISpanParsable<T>
            {
                var constructedObj4 = T.Parse(item.SemVerString.AsSpan(), provider: null /* unused */);
                _AssertConstructedProperties((SemanticVersion)(object)constructedObj4, item);

                Assert.True(T.TryParse(item.SemVerString.AsSpan(), provider: null /* unused */, out var constructedObj5));
                _AssertConstructedProperties((SemanticVersion)(object)constructedObj5, item);
            }
#endif

            static void _AssertConstructedProperties(SemanticVersion semanticVersion, SemVerTestItem item)
            {
                Assert.Equal(semanticVersion.ToString(), item.SemVerString);
                Assert.Equal(semanticVersion.Major.ToString(), item.ExpectedMajor);
                Assert.Equal(semanticVersion.Minor.ToString(), item.ExpectedMinor);
                Assert.Equal(semanticVersion.Patch.ToString(), item.ExpectedPatch);
                Assert.Equal(semanticVersion.Prerelease.ToString(), item.ExpectedPrerelease ?? string.Empty);
                Assert.Equal(semanticVersion.Build.ToString(), item.ExpectedBuild ?? string.Empty);
            }

            static void _AssertComponentsProperties(SemanticVersion.Components components, SemVerTestItem item, bool assertNoAllocation)
            {
                Assert.Equal(components.Major.ToString(), item.ExpectedMajor);
                Assert.Equal(components.Minor.ToString(), item.ExpectedMinor);
                Assert.Equal(components.Patch.ToString(), item.ExpectedPatch);
                Assert.Equal(components.Prerelease.ToString(), item.ExpectedPrerelease ?? string.Empty);
                Assert.Equal(components.Build.ToString(), item.ExpectedBuild ?? string.Empty);

                // Also assert that we didn't inadvertently allocate anything

                if (assertNoAllocation)
                {
                    Assert.True(components.Major.Overlaps(item.SemVerString.AsSpan()));
                    Assert.True(components.Minor.Overlaps(item.SemVerString.AsSpan()));
                    Assert.True(components.Patch.Overlaps(item.SemVerString.AsSpan()));
                    if (!string.IsNullOrEmpty(item.ExpectedPrerelease))
                    {
                        Assert.True(components.Prerelease.Overlaps(item.SemVerString.AsSpan()));
                    }
                    if (!string.IsNullOrEmpty(item.ExpectedBuild))
                    {
                        Assert.True(components.Build.Overlaps(item.SemVerString.AsSpan()));
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(CommonTestData.MalformedSemVerStrings), MemberType = typeof(CommonTestData))]
        public static void TestConstructionForMalformedInputs(string malformedString)
        {
            // Normal ctor

            ArgumentException ex;

            ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(malformedString));
            Assert.Equal("value", ex.ParamName);
            if (malformedString is null) { Assert.IsAssignableFrom<ArgumentNullException>(ex); }

            // Parse routines

            Assert.False(SemanticVersion.TryParse(malformedString, out var constructedObj1));
            Assert.Null(constructedObj1);

            Assert.False(SemanticVersion.TryParse(malformedString.AsSpan(), out var constructedObj2));
            Assert.Null(constructedObj2);

            ex = Assert.ThrowsAny<ArgumentException>(() => SemanticVersion.ParseIntoComponents(malformedString.AsSpan()));
            Assert.Equal("value", ex.ParamName);
            Assert.IsNotAssignableFrom<ArgumentNullException>(ex); // ANE on spans is nonsensical

            Assert.False(SemanticVersion.TryParseIntoComponents(malformedString.AsSpan(), out var components));
            _AssertComponentsProperties(components);

#if NET8_0_OR_GREATER
            _TestIParsableStaticInterfaceMethods<SemanticVersion>(malformedString);
            _TestISpanParsableStaticInterfaceMethods<SemanticVersion>(malformedString);
            static void _TestIParsableStaticInterfaceMethods<T>(string malformedString) where T : IParsable<T>
            {
                var ex = Assert.ThrowsAny<ArgumentException>(() => T.Parse(malformedString, provider: null /* unused */));
                Assert.Equal("s", ex.ParamName);
                if (malformedString is null) { Assert.IsAssignableFrom<ArgumentNullException>(ex); }

                Assert.False(T.TryParse(malformedString, provider: null /* unused */, out var constructedObj3));
                Assert.Null(constructedObj3);
            }
            static void _TestISpanParsableStaticInterfaceMethods<T>(string malformedString) where T : ISpanParsable<T>
            {
                var ex = Assert.ThrowsAny<ArgumentException>(() => T.Parse(malformedString.AsSpan(), provider: null /* unused */));
                Assert.Equal("s", ex.ParamName);
                Assert.IsNotAssignableFrom<ArgumentNullException>(ex); // ANE on spans is nonsensical

                Assert.False(T.TryParse(malformedString.AsSpan(), provider: null /* unused */, out var constructedObj3));
                Assert.Null(constructedObj3);
            }
#endif

            static void _AssertComponentsProperties(SemanticVersion.Components components)
            {
                Assert.True(components.Major.IsEmpty);
                Assert.True(components.Minor.IsEmpty);
                Assert.True(components.Patch.IsEmpty);
                Assert.True(components.Prerelease.IsEmpty);
                Assert.True(components.Build.IsEmpty);
            }
        }

        [Theory]
        [InlineData("10.20", null, null, "10.20.0")] // no build or revision
        [InlineData("10.20.30", null, null, "10.20.30")] // no revision
        [InlineData("10.20.30.0", null, null, "10.20.30")] // revision := 0 is ok
        [InlineData("100.200", "", "", "100.200.0")] // empty prerelease & build are ok
        [InlineData("100.200", "my-prerelease", null, "100.200.0-my-prerelease")]
        [InlineData("100.200.300", null, "my-build", "100.200.300+my-build")]
        [InlineData("600.700", "my-prerelease.12345", "my-build", "600.700.0-my-prerelease.12345+my-build")]
        [InlineData("600.700.800.0", "", "my-build", "600.700.800+my-build")]
        public static void TestConstructionFromSystemVersionString_Success(string systemVersionString, string prereleaseString, string buildString, string expectedSemVerString)
        {
            // Arrange & act

            SemanticVersion constructedSemVer = new SemanticVersion(new Version(systemVersionString), prereleaseString, buildString);
            Assert.Equal(expectedSemVerString, constructedSemVer.ToString());
        }

        [Fact]
        public static void TestConstructionFromSystemVersionString_NullVersion_Throws()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SemanticVersion((Version)null));
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public static void TestConstructionFromSystemVersionString_WithPositiveRevisionNumber_Throws()
        {
            Version v = new Version("12.34.56.78");
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(v));
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(SampleInvalidPrereleaseData))]
        public static void TestConstructionFromSystemVersionString_WithInvalidPrereleaseComponent_Throws(string prerelease)
        {
            Version v = new Version("1.2.3");
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(v, prerelease, build: null));
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(SampleInvalidBuildData))]
        public static void TestConstructionFromSystemVersionString_WithInvalidBuildComponent_Throws(string build)
        {
            Version v = new Version("1.2.3");
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(v, prerelease: null, build));
            Assert.Equal("build", ex.ParamName);
        }

        [Fact]
        public static void TestConstructionFromSemVerString_NullVersion_Throws()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SemanticVersion((SemanticVersion)null, null, null));
            Assert.Equal("baseVersion", ex.ParamName);
        }

        [Theory]
        [InlineData("1.2.3", null, null, "1.2.3")] // no build or revision -> no build or revision
        [InlineData("1.2.3", "", null, "1.2.3")] // empty treated same as null
        [InlineData("1.2.3", null, "", "1.2.3")] // empty treated same as null
        [InlineData("1.2.3", "abc", null, "1.2.3-abc")] // appends prerelease
        [InlineData("1.2.3-abc", "def", null, "1.2.3-def")] // replaces prerelease
        [InlineData("1.2.3+mybuild", "def", null, "1.2.3-def")] // adds prerelease, drops build
        [InlineData("1.2.3-abc+mybuild", "def", null, "1.2.3-def")] // replaces prerelease, drops build
        [InlineData("1.2.3", null, "mybuild", "1.2.3+mybuild")] // appends build
        [InlineData("1.2.3-abc", null, "mybuild", "1.2.3+mybuild")] // drops prerelease, appends build
        [InlineData("1.2.3+mybuild", null, "myotherbuild", "1.2.3+myotherbuild")] // replaces build
        [InlineData("1.2.3-abc+mybuild", null, "myotherbuild", "1.2.3+myotherbuild")] // drops prerelease, replaces build
        [InlineData("1.2.3-abc+mybuild", "def", "myotherbuild", "1.2.3-def+myotherbuild")] // replaces prerelease & build
        [InlineData("1.2.3-abc+mybuild", "", "", "1.2.3")] // drops all optional components
        public static void TestConstructionFromSemVerString_Success(string baseSemVerString, string prereleaseString, string buildString, string expectedSemVerString)
        {
            SemanticVersion baseVersion = new SemanticVersion(baseSemVerString); // may include dummy prerelease & build metadata
            SemanticVersion constructedSemVer = new SemanticVersion(baseVersion, prereleaseString, buildString); // overwrites prerelease & build metadata
            Assert.Equal(expectedSemVerString, constructedSemVer.ToString());
        }

        [Theory]
        [MemberData(nameof(SampleInvalidPrereleaseData))]
        public static void TestConstructionFromSemVerString_WithInvalidPrereleaseComponent_Throws(string prerelease)
        {
            SemanticVersion v = new SemanticVersion("1.2.3");
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(v, prerelease, build: null));
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(SampleInvalidBuildData))]
        public static void TestConstructionFromSemVerString_WithInvalidBuildComponent_Throws(string build)
        {
            SemanticVersion v = new SemanticVersion("1.2.3");
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(v, prerelease: null, build));
            Assert.Equal("build", ex.ParamName);
        }

        [Theory]
        [InlineData(0, 0, 0, null, null, "0.0.0")]
        [InlineData(0, 0, 1, "", null, "0.0.1")]
        [InlineData(0, 0, 2, null, "", "0.0.2")]
        [InlineData(0, 1, 0, "abc", null, "0.1.0-abc")]
        [InlineData(0, 1, 0, null, "def", "0.1.0+def")]
        [InlineData(0, 2, 0, "abc", "def", "0.2.0-abc+def")]
        [InlineData(100, 200, 300, null, null, "100.200.300")]
        [InlineData(400, 500, 600, "prerelease", "build", "400.500.600-prerelease+build")]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, null, null, "2147483647.2147483647.2147483647")]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, "my-prerelease", "my-build", "2147483647.2147483647.2147483647-my-prerelease+my-build")]
        public static void TestConstructionFromParts_Success(int major, int minor, int patch, string prerelease, string build, string expectedSemVerString)
        {
            SemanticVersion constructedSemVer = new SemanticVersion(major, minor, patch, prerelease, build);
            Assert.Equal(expectedSemVerString, constructedSemVer.ToString());
        }

        [Theory]
        [InlineData(-1, 200, 300, "major")]
        [InlineData(int.MinValue, 200, 300, "major")]
        [InlineData(100, -1, 300, "minor")]
        [InlineData(100, int.MinValue, 300, "minor")]
        [InlineData(100, 200, -1, "patch")]
        [InlineData(100, 200, int.MinValue, "patch")]
        public static void TestConstructionFromParts_BadIntComponents_Fail(int major, int minor, int patch, string expectedArgName)
        {
            var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new SemanticVersion(major, minor, patch));
            Assert.Equal(expectedArgName, ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(SampleInvalidPrereleaseData))]
        public static void TestConstructionFromParts_BadPrereleaseComponent_Fail(string prerelease)
        {
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(1, 2, 3, prerelease, null));
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(SampleInvalidBuildData))]
        public static void TestConstructionFromParts_BadBuildComponent_Fail(string build)
        {
            var ex = Assert.ThrowsAny<ArgumentException>(() => new SemanticVersion(1, 2, 3, null, build));
            Assert.Equal("build", ex.ParamName);
        }

        public static IEnumerable<object[]> SampleInvalidPrereleaseData()
        {
            return new[]
            {
                "00", // leading zeroes
                "0\0", // embedded null
                "1.2.3.00", // leading zeroes
                " ", // whitespace
                "a+b", // not alphanumeric char
                "a@b", // not alphanumeric char
                "1..2", // bad dot pattern
            }.Select(o => new[] { o });
        }

        public static IEnumerable<object[]> SampleInvalidBuildData()
        {
            return new[]
            {
                "0\0", // embedded null
                " ", // whitespace
                "a+b", // not alphanumeric char
                "a@b", // not alphanumeric char
                "1..2", // bad dot pattern
            }.Select(o => new[] { o });
        }
    }
}
