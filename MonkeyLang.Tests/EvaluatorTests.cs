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
                new object[] { "10", 10 }
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
                new object[] { "false", false }
            };

        [Theory]
        [MemberData(nameof(PrefixData))]
        public void Evaluate_CanEvalPrefixExpressions(string input, object expected)
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
            };

        private T AssertAndCast<T>(object obj) where T : class
        {
            Assert.IsType<T>(obj);
            return obj as T;
        }
    }
}
