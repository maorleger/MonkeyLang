﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace MonkeyLang.Tests
{
    public class EvaluatorTests
    {
        public EvaluatorTests()
        {
            this.subject = new Evaluator(new Parser(new Lexer()));
            this.environment = new RuntimeEnvironment();
        }

        private readonly Evaluator subject;
        private readonly RuntimeEnvironment environment;

        [Theory]
        [MemberData(nameof(IntegerData))]
        public void Evaluate_CanEvalIntegerExpressions(string input, int expected)
        {
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            IntegerObject intResult = this.AssertAndCast<IntegerObject>(actual);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            StringObject stringResult = this.AssertAndCast<StringObject>(actual);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);

            if (expected is ErrorObject expError)
            {
                ErrorObject errResult = this.AssertAndCast<ErrorObject>(actual);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            BooleanObject intResult = this.AssertAndCast<BooleanObject>(actual);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
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
            IObject actual = this.subject.Evaluate("let 5 x", this.environment);
            Assert.NotNull(actual);
            ErrorObject error = this.AssertAndCast<ErrorObject>(actual);
            Assert.Contains("Expected an identifier, got Token [Type='Int', Literal='5']", error.Messages);
        }

        [Theory]
        [MemberData(nameof(EvalErrorData))]
        public void Evaluate_CanHandleErrors(string input, string expected)
        {
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            ErrorObject error = this.AssertAndCast<ErrorObject>(actual);
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
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            IntegerObject intObj = this.AssertAndCast<IntegerObject>(actual);
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
            IObject actual = this.subject.Evaluate("fn(x) { x + 2; }", this.environment);
            FunctionObject fn = this.AssertAndCast<FunctionObject>(actual);
            Assert.Equal(1, fn.Parameters.Count);
            Assert.Equal("x", fn.Parameters[0].StringValue);
            Assert.Equal("(x + 2)", fn.Body.StringValue);
        }

        [Theory]
        [MemberData(nameof(FunctionData))]
        public void Evaluate_CanEvaluateFunction(string input, int expected)
        {
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            IntegerObject intObj = this.AssertAndCast<IntegerObject>(actual);
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

            IObject actual = this.subject.Evaluate(input, this.environment);
            IntegerObject intObj = this.AssertAndCast<IntegerObject>(actual);
            Assert.Equal(5, intObj.Value);
        }

        [Fact]
        public void Evaluate_CanEvaluateArrays()
        {
            string input = "[1, 2 * 2, 3 + 3]";

            IObject actual = this.subject.Evaluate(input, this.environment);
            ArrayObject arrayObject = this.AssertAndCast<ArrayObject>(actual);

            Assert.Equal(arrayObject.Elements[0], new IntegerObject(1));
            Assert.Equal(arrayObject.Elements[1], new IntegerObject(4));
            Assert.Equal(arrayObject.Elements[2], new IntegerObject(6));
        }

        [Theory]
        [MemberData(nameof(ArrayIndexingData))]
        public void Evaluate_CanEvaluateArrayIndexing(string input, IObject expected)
        {
            IObject actual = this.subject.Evaluate(input, this.environment);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> ArrayIndexingData =>
            new List<object[]>
            {
                new object[] { "[1, 2, 3][0]", new IntegerObject(1), },
                new object[] { "[1, 2, 3][1]", new IntegerObject(2), },
                new object[] { "[1, 2, 3][2]", new IntegerObject(3), },
                new object[] { "let i = 0; [1][i];", new IntegerObject(1), },
                new object[] { "[1, 2, 3][1 + 1];",  new IntegerObject(3) },
                new object[] { "let myArray = [1, 2, 3]; myArray[2];", new IntegerObject(3), },
                new object[] { "let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];", new IntegerObject(6) },
                new object[] { "let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]", new IntegerObject(2) },
                new object[] { "[1, 2, 3][3]", NullObject.Null },
                new object[] { "[1, 2, 3][-1]", NullObject.Null }
            };

        [Fact]
        public void Evaluate_CanEvaluateMap()
        {
            string input = @"
let map = fn(arr, f) {
    let iter = fn(arr, accumulated) {
        if (len(arr) == 0) {
            accumulated
        } else {
            iter(rest(arr), push(accumulated, f(first(arr))));
        }
    };
    iter(arr, []);
};
map([2,3], fn(x) { x * 2 });
";

            ArrayObject actual = this.AssertAndCast<ArrayObject>(this.subject.Evaluate(input, this.environment));
            Assert.Equal(2, actual.Elements.Count);
            IntegerObject intResult = this.AssertAndCast<IntegerObject>(actual.Elements[0]);
            Assert.Equal(4, intResult.Value);
            intResult = this.AssertAndCast<IntegerObject>(actual.Elements[1]);
            Assert.Equal(6, intResult.Value);
        }

        [Fact]
        public void Evaluate_CanEvaluateHashLiteral()
        {
            string input = @"
let two = ""two"";
{
    ""one"": 10 - 9,
    two: 1 + 1,
    ""thr"" + ""ee"": 6/ 2,
    4: 4,
    true: 5,
    false: 6
}
";

            HashObject actual = this.AssertAndCast<HashObject>(this.subject.Evaluate(input, this.environment));

            var expected = new Dictionary<IObject, IObject>()
            {
                { new StringObject("one"), new IntegerObject(1) },
                { new StringObject("two"), new IntegerObject(2) },
                { new StringObject("three"), new IntegerObject(3) },
                { new IntegerObject(4), new IntegerObject(4) },
                { BooleanObject.True, new IntegerObject(5) },
                { BooleanObject.False, new IntegerObject(6) }
            }.ToImmutableDictionary();

            Assert.Equal(expected, actual.Pairs);
        }

        [Fact]
        public void Evaluate_CanEvaluateHashIndexing()
        {
            string input = @"
let two = ""two"";
let h = {
    ""one"": 10 - 9,
    two: 1 + 1,
    ""thr"" + ""ee"": 6/ 2,
    4: 4,
    true: 5,
    false: 6
};
h[1 + 3];
";

            IntegerObject actual = this.AssertAndCast<IntegerObject>(this.subject.Evaluate(input, this.environment));
            Assert.Equal(4, actual.Value);
        }

        private T AssertAndCast<T>(object obj) where T : class
        {
            Assert.IsType<T>(obj);
            return obj as T;
        }
    }
}
