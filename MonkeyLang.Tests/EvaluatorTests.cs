using Microsoft.VisualBasic;
using Pidgin;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace MonkeyLang.Tests
{
    public class EvaluatorTests
    {
        public EvaluatorTests()
        {
            subject = new Evaluator(new Parser(new Lexer()));
        }

        private readonly Evaluator subject;

        [Theory]
        [MemberData(nameof(IntegerData))]
        public void Evaluate_CanEvalIntegerExpressions(string input, int expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            var intResult = AssertAndCast<IntegerObject>(actual);
            Assert.Equal(expected, intResult.Value);
        }

        public static IEnumerable<object[]> IntegerData =>
            new List<object[]>
            {
                new object[] { "5", 5 },
                new object[] { "10", 10 },
                new object[] {"5 + 5 + 5 + 5 - 10", 10},
                new object[] {"2 * 2 * 2 * 2 * 2", 32},
                new object[] {"-50 + 100 + -50", 0},
                new object[] {"5 * 2 + 10", 20},
                new object[] {"5 + 2 * 10", 25},
                new object[] {"20 + 2 * -10", 0},
                new object[] {"50 / 2 * 2 + 10", 60},
                new object[] {"2 * (5 + 10)", 30},
                new object[] {"3 * 3 * 3 + 10", 37},
                new object[] {"3 * (3 * 3) + 10", 37},
                new object[] {"(5 + 10 * 2 + 15 / 3) * 2 + -10", 50}
            };

        [Theory]
        [MemberData(nameof(BooleanData))]
        public void Evaluate_CanEvalBooleanExpressions(string input, bool expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            var intResult = AssertAndCast<BooleanObject>(actual);
            Assert.Equal(expected, intResult.Value);
        }

        public static IEnumerable<object[]> BooleanData =>
            new List<object[]>
            {
                new object[] { "true", true },
                new object[] { "false", false },
                new object[] {"1 < 2", true},
                new object[] {"1 > 2", false},
                new object[] {"1 < 1", false},
                new object[] {"1 > 1", false},
                new object[] {"1 == 1", true},
                new object[] {"1 != 1", false},
                new object[] {"1 == 2", false},
                new object[] {"1 != 2", true},
                new object[] {"true == true", true},
                new object[] {"false == false", true},
                new object[] {"true == false", false},
                new object[] {"true != false", true},
                new object[] {"false != true", true},
                new object[] {"(1 < 2) == true", true},
                new object[] {"(1 < 2) == false", false},
                new object[] {"(1 > 2) == true", false},
                new object[] {"(1 > 2) == false", true},
            };

        [Theory]
        [MemberData(nameof(PrefixData))]
        public void Evaluate_CanEvalPrefixExpressions(string input, IObject expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            Assert.IsType(expected.GetType(), actual);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> PrefixData =>
            new List<object[]>
            {
                new object[] { "!true", BooleanObject.False },
                new object[] { "!false", BooleanObject.True },
                new object[] { "!!true", BooleanObject.True },
                new object[] { "!!false", BooleanObject.False },
                new object[] { "!!5", BooleanObject.True },
                new object[] { "-5", new IntegerObject(-5) },
                new object[] { "-10", new IntegerObject(-10) },
                new object[] { "-true", NullObject.Null }
            };

        [Theory]
        [MemberData(nameof(ConditionalData))]
        public void Evaluate_CanEvalConditionalExpressions(string input, IObject expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            Assert.IsType(expected.GetType(), actual);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> ConditionalData =>
            new List<object[]>
            {
                new object[] {"if (true) { 10 }", new IntegerObject(10) },
                new object[] {"if (false) { 10 }", NullObject.Null },
                new object[] {"if (1) { 10 }", new IntegerObject(10) },
                new object[] {"if (1 < 2) { 10 }", new IntegerObject(10) },
                new object[] {"if (1 > 2) { 10 }", NullObject.Null },
                new object[] {"if (1 > 2) { 10 } else { 20 }", new IntegerObject(20) },
                new object[] {"if (1 < 2) { 10 } else { 20 }", new IntegerObject(10) },
            };

        private T AssertAndCast<T>(object obj) where T : class
        {
            Assert.IsType<T>(obj);
            return obj as T;
        }
    }
}
