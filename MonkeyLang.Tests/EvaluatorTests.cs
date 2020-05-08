using Microsoft.VisualBasic;
using Pidgin;
using System;
using System.Collections.Generic;
using System.Linq;
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
                new object[] { "5 + 5 + 5 + 5 - 10", 10 },
                new object[] { "2 * 2 * 2 * 2 * 2", 32 },
                new object[] { "-50 + 100 + -50", 0 },
                new object[] { "5 * 2 + 10", 20 },
                new object[] { "5 + 2 * 10", 25 },
                new object[] { "20 + 2 * -10", 0 },
                new object[] { "50 / 2 * 2 + 10", 60 },
                new object[] { "2 * (5 + 10)", 30 },
                new object[] { "3 * 3 * 3 + 10", 37 },
                new object[] { "3 * (3 * 3) + 10", 37 },
                new object[] { "(5 + 10 * 2 + 15 / 3) * 2 + -10", 50 }
            };

        [Theory]
        [MemberData(nameof(StringData))]
        public void Evaluate_CanEvalStringExpressions(string input, string expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            var stringResult = AssertAndCast<StringObject>(actual);
            Assert.Equal(expected, stringResult.Value);
        }

        public static IEnumerable<object[]> StringData =>
            new List<object[]>
            {
                new object[] { "\"foo bar\"", "foo bar" },
                new object[] { "\"foo\" + \"bar\"", "foobar" }
            };

        [Theory]
        [MemberData(nameof(BuiltInFunctionData))]
        public void Evaluate_CanEvalBuiltInFunctions(string input, IObject expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);

            if (expected is ErrorObject expError)
            {
                var errResult = AssertAndCast<ErrorObject>(actual);
                Assert.Equal(1, errResult.Messages.Count);
                Assert.Equal(expError.Messages[0], errResult.Messages[0]);
            }
            else
            {
                Assert.Equal(expected, actual);
            }
        }

        public static IEnumerable<object[]> BuiltInFunctionData =>
            new List<object[]>
            {
                new object[] { "len(\"\")", new IntegerObject(0)},
                new object[] { "len(\"four\")", new IntegerObject(4)},
                new object[] { "len(\"hello world\")", new IntegerObject(11)},
                new object[] { "len(1)", new ErrorObject(new[] { "argument to \"len\" not supported, got Integer" }) },
                new object[] { "len(\"one\", \"two\")", new ErrorObject(new[] { "wrong number of arguments. got=2, want=1" }) }
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
                new object[] { "true", true  },
                new object[] { "false", false  },
                new object[] { "1 < 2", true },
                new object[] { "1 > 2", false },
                new object[] { "1 < 1", false },
                new object[] { "1 > 1", false },
                new object[] { "1 == 1", true },
                new object[] { "1 != 1", false },
                new object[] { "1 == 2", false },
                new object[] { "1 != 2", true },
                new object[] { "true == true", true },
                new object[] { "false == false", true },
                new object[] { "true == false", false },
                new object[] { "true != false", true },
                new object[] { "false != true", true },
                new object[] { "(1 < 2) == true", true },
                new object[] { "(1 < 2) == false", false },
                new object[] { "(1 > 2) == true", false },
                new object[] { "(1 > 2) == false", true },
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
                new object[] { "if (true) {  10 }", new IntegerObject(10) },
                new object[] { "if (false) {  10 }", NullObject.Null },
                new object[] { "if (1) {  10 }", new IntegerObject(10) },
                new object[] { "if (1 < 2) {  10 }", new IntegerObject(10) },
                new object[] { "if (1 > 2) {  10 }", NullObject.Null },
                new object[] { "if (1 > 2) {  10 } else {  20 }", new IntegerObject(20) },
                new object[] { "if (1 < 2) {  10 } else {  20 }", new IntegerObject(10) },
            };

        [Theory]
        [MemberData(nameof(ReturnData))]
        public void Evaluate_CanEvalReturnStatements(string input, IObject expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            Assert.IsType(expected.GetType(), actual);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> ReturnData =>
            new List<object[]>
            {
                new object[] { "return 10;", new IntegerObject(10) },
                new object[] { "return 10; 9;", new IntegerObject(10) },
                new object[] { "return 2 + 5; 9", new IntegerObject(7) },
                new object[] { "9; return 2 + 5; 9", new IntegerObject(7) },
                new object[]
                {
                    "if (10 > 1) { if (10 > 1) { return 10; } return 1; }",
                    new IntegerObject(10)
                }
            };

        [Fact]
        public void Evaluate_CanEvalParseErrors()
        {
            var actual = subject.Evaluate("let 5 x");
            Assert.NotNull(actual);
            var error = AssertAndCast<ErrorObject>(actual);
            Assert.Contains("Expected an identifier, got Token [Type='Int', Literal='5']", error.Messages);
        }

        [Theory]
        [MemberData(nameof(EvalErrorData))]
        public void Evaluate_CanHandleErrors(string input, string expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            var error = AssertAndCast<ErrorObject>(actual);
            Assert.Contains(expected, error.Messages);
        }

        public static IEnumerable<object[]> EvalErrorData =>
            new List<object[]>
            {
                new object[] {
                    "5 + true;",
                    "type mismatch: Integer + Boolean",
                },
                new object[]
                {
                    "5 + true; 5;",
                    "type mismatch: Integer + Boolean",
                },
                new object[]
                {
                    "-true",
                    "unknown operator: -Boolean",
                },
                new object[] {
                    "true + false;",
                    "unknown operator: Boolean + Boolean",
                },
                new object[]
                {
                    "5; true + false; 5",
                    "unknown operator: Boolean + Boolean",
                },
                new object[]
                {
                    "if (10 > 1) { true + false; }",
                    "unknown operator: Boolean + Boolean",
                },
                new object[]
                {
                    "if (10 > 1) { if (10 > 1) { return true + false } } return 1; }",
                    "unknown operator: Boolean + Boolean",
                },
                new object[]
                {
                    "foobar",
                    "identifier not found: foobar",
                }
            };

        [Theory]
        [MemberData(nameof(LetStatementData))]
        public void Evaluate_CanEvaluateLetStatements(string input, int expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            var intObj = AssertAndCast<IntegerObject>(actual);
            Assert.Equal(expected, intObj.Value);
        }

        public static IEnumerable<object[]> LetStatementData =>
            new List<object[]>
            {
                new object[] { "let a = 5; a;", 5 },
                new object[] { "let a = 5; let b = a; b;", 5 },
                new object[] { "let a = 5 + 5; let b = a; let c = a + b + 5; c;", 25 }
            };

        [Fact]
        public void Evaluate_CanEvaluateFunctionObject()
        {
            var actual = subject.Evaluate("fn(x) { x + 2; }");
            var fn = AssertAndCast<FunctionObject>(actual);
            Assert.Equal(1, fn.Parameters.Count);
            Assert.Equal("x", fn.Parameters[0].StringValue);
            Assert.Equal("(x + 2)", fn.Body.StringValue);
        }

        [Theory]
        [MemberData(nameof(FunctionData))]
        public void Evaluate_CanEvaluateFunction(string input, int expected)
        {
            var actual = subject.Evaluate(input);
            Assert.NotNull(actual);
            var intObj = AssertAndCast<IntegerObject>(actual);
            Assert.Equal(expected, intObj.Value);
        }

        public static IEnumerable<object[]> FunctionData =>
            new List<object[]>
            {
                new object[] { "let identity = fn(x) {  x;  }; identity(5);", 5 },
                new object[] { "let identity = fn(x) {  return x;  }; identity(5);", 5 },
                new object[] { "let double = fn(x) {  x * 2;  }; double(5);", 10 },
                new object[] { "let add = fn(x, y) {  x + y;  }; add(5, 5);", 10 },
                new object[] { "let add = fn(x, y) {  x + y;  }; add(5 + 5, add(5, 5));", 20 },
                new object[] { "fn(x) {  x;  }(5)", 5 },
            };

        [Fact]
        public void Evaluate_CanEvaluateClosures()
        {
            string input = @"
let newAdder = fn(x) { 
    fn(y) { x + y };
};

let addTwo = newAdder(2);
let addThree = newAdder(3);
addTwo(3);
";

            var actual = subject.Evaluate(input);
            var intObj = AssertAndCast<IntegerObject>(actual);
            Assert.Equal(5, intObj.Value);
        }

        private T AssertAndCast<T>(object obj) where T : class
        {
            Assert.IsType<T>(obj);
            return obj as T;
        }
    }
}
