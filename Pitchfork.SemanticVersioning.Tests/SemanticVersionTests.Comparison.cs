using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;
using Xunit.Sdk;

namespace Pitchfork.SemanticVersioning.Tests
{
    public static partial class SemanticVersionTests
    {
        [Fact]
        public static void TestComparisonsAndEquality()
        {
            List<SemVerTestItem> orderedTestItems = CommonTestData.GetProperlyOrderedWellFormedTestData().ToList();
            orderedTestItems.Insert(0, null); // null always comes before everything else

            for (int i = 0; i < orderedTestItems.Count; i++)
            {
                for (int j = 0; j < orderedTestItems.Count; j++)
                {
                    var left = orderedTestItems[i]?.ToSemanticVersion();
                    var right = orderedTestItems[j]?.ToSemanticVersion();
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

        private static void _TestComparisonRunner(SemanticVersion left, SemanticVersion right, int expectedSign)
        {
            // Equality and hash code implementations

            if (expectedSign == 0)
            {
                Assert.True(Equals(left, right));
                Assert.True(EqualityComparer<SemanticVersion>.Default.Equals(left, right));

                if (left is not null)
                {
                    Assert.True(((object)left).Equals(right));
                    Assert.True(left.Equals(right));
                }

                Assert.Equal(left?.GetHashCode(), right?.GetHashCode());
            }
            else
            {
                Assert.False(Equals(left, right));
                Assert.False(EqualityComparer<SemanticVersion>.Default.Equals(left, right));

                if (left is not null)
                {
                    Assert.False(((object)left).Equals(right));
                    Assert.False(left.Equals(right));
                }

                // n.b. GetHashCode not required to be different for unequal objects
            }

            // IComparison implementations

            if (left is not null)
            {
                Assert.Equal(expectedSign, Math.Sign(left.CompareTo(right)));
                Assert.Equal(expectedSign, Math.Sign(((IComparable)left).CompareTo(right)));
                Assert.Equal(expectedSign, Math.Sign(((IComparable<SemanticVersion>)left).CompareTo(right)));
            }

            // Operators

            bool expectedLessThan = (expectedSign < 0);
            Assert.Equal(expectedLessThan, left < right);

            bool expectedLessThanOrEqualTo = (expectedSign <= 0);
            Assert.Equal(expectedLessThanOrEqualTo, left <= right);

            bool expectedGreaterThan = (expectedSign > 0);
            Assert.Equal(expectedGreaterThan, left > right);

            bool expectedGreaterThanOrEqualTo = (expectedSign >= 0);
            Assert.Equal(expectedGreaterThanOrEqualTo, left >= right);

            bool expectedEqual = (expectedSign == 0);
            Assert.Equal(expectedEqual, left == right);

            bool expectedNotEqual = (expectedSign != 0);
            Assert.Equal(expectedNotEqual, left != right);

#if NET8_0_OR_GREATER
            // .NET 8 generic operators

            GenericOperatorsTests<SemanticVersion>(left, right);
            void GenericOperatorsTests<T>(T left, T right) where T : IComparisonOperators<T, T, bool>
            {
                Assert.Equal(expectedLessThan, left < right);
                Assert.Equal(expectedLessThanOrEqualTo, left <= right);
                Assert.Equal(expectedGreaterThan, left > right);
                Assert.Equal(expectedGreaterThanOrEqualTo, left >= right);
                Assert.Equal(expectedEqual, left == right);
                Assert.Equal(expectedNotEqual, left != right);
            }
#endif
        }
    }
}
