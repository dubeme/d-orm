using Microsoft.VisualStudio.TestTools.UnitTesting;
using DamnORM.Model;
using DamnORM.unit.tests.Model.POCOs;
using System;
using System.Text.RegularExpressions;

namespace DamnORM.unit.tests.Model.SQL
{
    [TestClass]
    public class SqlExpressionTest
    {
        private const string SIMPLE_EXPRESSION = "Simple expression";
        private const string EXPRESSION_WITH_VALUES_MISSING = "Simple expression with values missing";
        private const string EXPRESSION_WITH_CIRCULAR_REFERENCE = "Simple expression with circular reference";
        private const string IDENTIFIER_REGEX = @"([a-z][\w]*)";
        private const string COMPARISON_OPERATOR_REGEX = @"(=|>|<|>=|<=|<>|!<|!>|like)";
        private const string LOGICAL_OPERATOR_REGEX = @"(and|or)";
        private const string NUMBER_REGEX = @"([\d]+)";
        private static string WHERE_EXPRESSION_GUID;

        private const int TEST_NUMBER = 9;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            /*
             *
             * ^(\({0} {1} @{0}_{2}\))( {3} (\({0} {1} @{0}_{2}\)))*$
             *
             * ^
             * ({0} {1} @{0}_{2})            => (ID COMP_OPER @ID_NUMBER)
             * ( {3} ({0} {1} @{0}_{2}))     => ( LOG_OPER ID COMP_OPER @ID_NUMBER)
             * $
             *
             *
             *
             * \[?({0}|(@{0}_{2}))\]?
             *
             *
             */

            WHERE_EXPRESSION_GUID = string.Format(
                @"^(\({0} {1} {0}\))( {2} (\({0} {1} {0}\)))*$"
                , string.Format(@"((\[?{0}\]?)|(@{0}))", IDENTIFIER_REGEX)
                , COMPARISON_OPERATOR_REGEX
                , LOGICAL_OPERATOR_REGEX);
        }

        private static bool MatchesExpressionPattern(string actual)
        {
            return Regex.IsMatch(actual, WHERE_EXPRESSION_GUID, RegexOptions.IgnoreCase);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestEqualComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.Age == TEST_NUMBER);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestGreaterThanComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.Age > TEST_NUMBER);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestLessThanComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.Age < TEST_NUMBER);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestGreaterThanOrEqualToComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.Age >= TEST_NUMBER);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestLessThanOrEqualToComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.Age <= TEST_NUMBER);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestNotEqualToComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.Age != TEST_NUMBER);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestNotLessThanComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => !(p.Age < TEST_NUMBER));

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestNotGreaterThanComparison()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => !(p.Age > TEST_NUMBER));

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestLikeComparison()
        {
            //var expr = BetterExpression<Person>
            //    .BuildExpression(p => p.Age, SqlComparisonOperator.Like);

            //var isAMatch = MatchesExpressionPattern(expr.ToString());
            //Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestJoinedExpressions1()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => p.FirstName == null && p.LastName != null);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(SIMPLE_EXPRESSION)]
        public void TestJoinedExpressions2()
        {
            var expr1 = ExpressionParseHelper<Person>.Parse(p =>
                    (p.FirstName == null && p.LastName != null) ||
                    (p.Gender == "F" && p.ID <= TEST_NUMBER));

            var isAMatch = MatchesExpressionPattern(expr1.ToString());
            Assert.IsTrue(isAMatch);
        }

        [TestMethod]
        [TestCategory(EXPRESSION_WITH_VALUES_MISSING)]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Column_Null()
        {
            var expr = ExpressionParseHelper<Person>.Parse(p => null == SIMPLE_EXPRESSION);

            var isAMatch = MatchesExpressionPattern(expr.ToString());
            Assert.IsTrue(isAMatch);
        }
    }
}