using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Pitchfork.SemanticVersioning.Tests
{
    public static class CommonTestData
    {
        public static IEnumerable<object[]> WellFormedTestData => GetProperlyOrderedWellFormedTestData().Select(x => new[] { x });

        public static IEnumerable<SemVerTestItem> GetProperlyOrderedWellFormedTestData()
        {
            yield return new("0.0.0-0", "0", "0", "0", "0"); // prerelease sorts before no prerelease
            yield return new("0.0.0-0+0", "0", "0", "0", "0", "0"); // build metadata sorts after no build metadata
            yield return new("0.0.0", "0", "0", "0");
            yield return new("0.0.0+0", "0", "0", "0", null, "0"); // build metadata sorts after no build metadata
            yield return new("0.0.1", "0", "0", "1");
            yield return new("0.0.2", "0", "0", "2");
            yield return new("0.0.10", "0", "0", "10"); // numeric comparison on patch, not ordinal comparison
            yield return new("0.0.2147483647", "0", "0", "2147483647"); // int32.MaxValue should be ok
            yield return new("0.0.2147483648", "0", "0", "2147483648"); // int32.MaxValue + 1 should be ok
            yield return new("0.0.18446744073709551616", "0", "0", "18446744073709551616"); // 2**64 should be ok
            yield return new("0.1.0", "0", "1", "0");
            yield return new("0.2.0", "0", "2", "0");
            yield return new("0.2.1", "0", "2", "1");
            yield return new("0.2.2-preview", "0", "2", "2", "preview"); // ordinal sort on alphanumeric prerelease
            yield return new("0.2.2-preview1", "0", "2", "2", "preview1");
            yield return new("0.2.2-preview10", "0", "2", "2", "preview10"); // ordinal sort on alphanumeric prerelease
            yield return new("0.2.2-preview2", "0", "2", "2", "preview2");
            yield return new("0.2.2-preview2+10", "0", "2", "2", "preview2", "10");
            yield return new("0.2.2-preview2+8", "0", "2", "2", "preview2", "8"); // ordinal sort on build
            yield return new("0.2.2", "0", "2", "2"); // non-prerelease sorts before prerelease
            yield return new("0.10.0", "0", "10", "0"); // numeric comparison on minor, not ordinal comparison
            yield return new("0.2147483647.0", "0", "2147483647", "0"); // int32.MaxValue should be ok
            yield return new("0.2147483648.0", "0", "2147483648", "0"); // int32.MaxValue + 1 should be ok
            yield return new("0.18446744073709551616.0", "0", "18446744073709551616", "0"); // 2**64 should be ok
            yield return new("1.0.0", "1", "0", "0");
            yield return new("1.2.3", "1", "2", "3");
            yield return new("1.2.3+0", "1", "2", "3", null, "0"); // build metadata sorts after no build metadata
            yield return new("2.3.4-0", "2", "3", "4", "0");
            yield return new("2.3.4-1", "2", "3", "4", "1");
            yield return new("2.3.4-2", "2", "3", "4", "2");
            yield return new("2.3.4-3", "2", "3", "4", "3");
            yield return new("2.3.4-4", "2", "3", "4", "4");
            yield return new("2.3.4-5", "2", "3", "4", "5");
            yield return new("2.3.4-6", "2", "3", "4", "6");
            yield return new("2.3.4-7", "2", "3", "4", "7");
            yield return new("2.3.4-8", "2", "3", "4", "8");
            yield return new("2.3.4-9", "2", "3", "4", "9");
            yield return new("2.3.4-10", "2", "3", "4", "10");
            yield return new("2.3.4-50", "2", "3", "4", "50");
            yield return new("2.3.4--", "2", "3", "4", "-"); // "-" is a valid alphanumeric string, sorts after numeric
            yield return new("2.3.4-0-", "2", "3", "4", "0-"); // "0-" is valid alphanumeric string; "0" sorts after "-" ordinally
            yield return new("2.3.4-0-0", "2", "3", "4", "0-0"); // "0-0" is valid alphanumeric string
            yield return new("2.3.4-A", "2", "3", "4", "A");
            yield return new("2.3.4-Z", "2", "3", "4", "Z");
            yield return new("2.3.4-a", "2", "3", "4", "a"); // "a" sorts after "Z" ordinally
            yield return new("2.3.4-z", "2", "3", "4", "z");
            yield return new("3.0.0+-", "3", "0", "0", null, "-"); // build strings sort ordinally, -, then 0..9, then A..Z, then a..z
            yield return new("3.0.0+0", "3", "0", "0", null, "0");
            yield return new("3.0.0+0-0", "3", "0", "0", null, "0-0"); // "0-..." sorts before "00..." ordinally
            yield return new("3.0.0+00", "3", "0", "0", null, "00"); // build metadata is always alphanumeric and allows leading zeroes
            yield return new("3.0.0+01", "3", "0", "0", null, "01");
            yield return new("3.0.0+10", "3", "0", "0", null, "10");
            yield return new("3.0.0+9", "3", "0", "0", null, "9");
            yield return new("3.0.0+A", "3", "0", "0", null, "A");
            yield return new("3.0.0+Z", "3", "0", "0", null, "Z");
            yield return new("3.0.0+a", "3", "0", "0", null, "a");
            yield return new("3.0.0+z", "3", "0", "0", null, "z");
            yield return new("4.0.0-1000", "4", "0", "0", "1000"); // numeric sort on prerelease segment
            yield return new("4.0.0-1000.0", "4", "0", "0", "1000.0");
            yield return new("4.0.0-alpha.0", "4", "0", "0", "alpha.0"); // "alpha" comes after "1000" (numeric)
            yield return new("4.0.0-alpha.5", "4", "0", "0", "alpha.5");
            yield return new("4.0.0-alpha.10", "4", "0", "0", "alpha.10"); // numeric sort on prerelease segment
            yield return new("4.0.0-alpha.-0", "4", "0", "0", "alpha.-0"); // "-0" turns into alphanumeric sort
            yield return new("4.0.0-alpha.-00", "4", "0", "0", "alpha.-00");
            yield return new("4.0.0-alpha.-10", "4", "0", "0", "alpha.-10");
            yield return new("4.0.0-alpha.-5", "4", "0", "0", "alpha.-5");
            yield return new("4.0.0-alpha.0-", "4", "0", "0", "alpha.0-");
            yield return new("4.0.0-alpha.00-", "4", "0", "0", "alpha.00-");
            yield return new("4.0.0-alpha.10-", "4", "0", "0", "alpha.10-");
            yield return new("4.0.0-alpha.5-", "4", "0", "0", "alpha.5-");
            yield return new("4.0.0-alpha.A", "4", "0", "0", "alpha.A");
            yield return new("4.0.0-alpha.Z", "4", "0", "0", "alpha.Z");
            yield return new("4.0.0-alpha.a", "4", "0", "0", "alpha.a");
            yield return new("4.0.0-alpha.z", "4", "0", "0", "alpha.z");
            yield return new("5.0.0-1.alpha.2.bravo+3.charlie.4.delta", "5", "0", "0", "1.alpha.2.bravo", "3.charlie.4.delta");
            yield return new("10.0.0", "10", "0", "0");
            yield return new("10.0.0+00000", "10", "0", "0", null, "00000"); // leading zeroes in build metadata
            yield return new("100.0.0-2147483647+2147483647", "100", "0", "0", "2147483647", "2147483647"); // int32.MaxValue should be ok
            yield return new("100.0.0-10000000000+10000000000", "100", "0", "0", "10000000000", "10000000000"); // arbitrary large number with leading 1
            yield return new("100.0.0-10000000000+5", "100", "0", "0", "10000000000", "5"); // builds always treated as alphanumeric
            yield return new("2147483647.2147483647.2147483647", "2147483647", "2147483647", "2147483647"); // int32.MaxValue should be ok
            yield return new("2147483648.2147483648.2147483648", "2147483648", "2147483648", "2147483648"); // int32.MaxValue + 1 should be ok
            yield return new("18446744073709551616.18446744073709551616.18446744073709551616", "18446744073709551616", "18446744073709551616", "18446744073709551616"); // 2**64 should be ok
        }

        public static IEnumerable<object?[]> MalformedSemVerStrings => GetMalformedSemVerStrings().Select(x => new[] { x });

        public static IEnumerable<string?> GetMalformedSemVerStrings()
        {
            yield return null;
            yield return string.Empty;
            yield return " ";

            // whitespace disallowed
            yield return " 1.2.3 ";
            yield return "1 .2.3";
            yield return "1.2 .3";
            yield return "1.2.3 ";
            yield return "1.2.3-preview 1";
            yield return "1.2.3-preview+111 ";
            yield return "1.2.3-preview+222 333";

            // too few components
            yield return "1";
            yield return "1.2";
            yield return "1.2.3-"; // missing prerelease string
            yield return "1.2.3-prerelease+"; // missing build string
            yield return "1.2.3+"; // missing build string

            // leading zeroes
            yield return "01.2.3";
            yield return "1.02.3";
            yield return "1.2.03";
            yield return "1.2.3-00"; // yes, even in non-alpha prerelease segments!
            yield return "1.2.3-01";
            yield return "1.2.3-alpha1.00";
            yield return "1.2.3-alpha1.01";
            yield return "1.2.3-00.alpha1";

            // misplaced dots
            yield return ".1.2.3";
            yield return "1..2.3";
            yield return "1.2..3";
            yield return "1.2.3.";
            yield return "1.2.3-preview..1"; // yes, even in prerelease segments!
            yield return "1.2.3-preview1+111..222"; // yes, even in build segments!
            yield return "1.2.3-preview.";
            yield return "1.2.3+build.";

            // negative numbers
            // (n.b. 1.2.3-alpha.-1 is ok since the "-1" there is alphanumeric)
            yield return "-1.2.3";
            yield return "1.-2.3";
            yield return "1.2.-3";

            // control chars
            yield return "1\r.2.3";
            yield return "1.2\n.3";
            yield return "1.2.3\t";

            // plus only allowed as build separator
            yield return "+1.2.3";
            yield return "1.2.3+00+11";

            // trailing nulls (int.TryParse normally allows this)
            yield return "1\0.2.3";
            yield return "1.2\0.3";
            yield return "1.2.3\0";

            // non-ASCII decimal digits
            yield return "\uFF11.2.3"; // U+FF11 = FULLWIDTH DIGIT ONE 
            yield return "1.\u0F22.3"; // U+0F22 = TIBETAN DIGIT TWO
            yield return "1.2.\u0D69"; // U+0D69 = MALAYALAM DIGIT THREE
            yield return "1.2.3-preview.\u0C6A"; // U+0C6A = TELUGU DIGIT FOUR
            yield return "1.2.3-preview.4+\u096B"; // U+096B = DEVANAGARI DIGIT FIVE
        }
    }
}
