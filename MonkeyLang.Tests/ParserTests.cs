using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace MonkeyLang.Tests
{
    public class ParserTests
    {
        public ParserTests()
        {
            subject = new Parser(new Lexer());
        }

        private readonly Parser subject;

        [Fact]
        public void Parser_CanParseLetStatements()
        {
            var input = @"
    let x = 5 + 6;
    let y = 10;
    let foobar = hello;
";

            Program actual = subject.ParseProgram(input).Program;
            Assert.NotNull(actual.Statements);
            Assert.Equal(3, actual.Statements.Count());

            var expected = new []
            {
                "x",
                "y",
                "foobar"
            };

            for (int i = 0; i < actual.Statements.Count(); i++)
            {
                var element = actual.Statements.ElementAt(i);
                Assert.IsType<LetStatement>(element);
                Assert.Equal(expected[i], ((LetStatement)element).Name.TokenLiteral);
                // TODO: test for the RHS
            }
        }

        [Fact]
        public void Parser_CanListParseErrors()
        {
            var input = @"
let x 5;
let y = 10;
let = 10;
let 838383;
";

            AST actual = subject.ParseProgram(input);

            Assert.NotEmpty(actual.Errors);
            Assert.Equal(1, actual.Program.Statements.Count);
        }

        [Fact]
        public void Parser_CanParseReturnStatements()
        {
            var input = @"
return 5;
return fn(x,y) { x + y };
return xyz;
";

            AST actual = subject.ParseProgram(input);

            Assert.Equal(3, actual.Program.Statements.Count);
            Assert.Empty(actual.Errors);
            Assert.All(actual.Program.Statements, stmt => Assert.IsType<ReturnStatement>(stmt));
                // TODO: test for the RHS
        }

        [Fact]
        public void Parser_CanParseIdentifiers()
        {
            var input = @"
foobar;
";
            AST result = subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);

            Assert.IsType<Identifier>(actual.Expression);
            Assert.Equal("foobar", actual.TokenLiteral);
            
        }

        [Fact]
        public void Parser_CanParseIntegerLiterals()
        {
            var input = @"
5;
";
            AST result = subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var intExpression = AssertAndCast<IntegerLiteral>(actual.Expression);
            Assert.Equal(5, intExpression.Value);
            
            Assert.Equal("5", actual.TokenLiteral);
        }

        [Fact]
        public void Parser_CanParseBooleans()
        {
            var input = @"
true;
false
";

            AST result = subject.ParseProgram(input);

            Assert.Equal(2, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var boolExpression = AssertAndCast<Boolean>(actual.Expression);
            Assert.True(boolExpression.Value);

            actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[1]);
            boolExpression = AssertAndCast<Boolean>(actual.Expression);
            Assert.False(boolExpression.Value);
        }

        [Fact]
        public void Parser_CanParseIfExpressions()
        {
            var input = @"
if (x < y) { x }
";

            AST result = subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var ifExpression = AssertAndCast<IfExpression>(actual.Expression);
            Assert.Equal("(x < y)", ifExpression.Condition.StringValue);

            Assert.Equal(1, ifExpression.Consequence.Statements.Count);
            var consequenceExpression = AssertAndCast<ExpressionStatement>(ifExpression.Consequence.Statements[0]);
            var consequenceIdentifier = AssertAndCast<Identifier>(consequenceExpression.Expression);
            Assert.Equal("x", consequenceIdentifier.Value);

            Assert.Null(ifExpression.Alternative);
        }

        [Fact]
        public void Parser_CanParseFunctionLiteral()
        {
            var input = @"
fn(x, y) { x + y; }
";

            AST result = subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var fnExpression = AssertAndCast<FunctionLiteral>(actual.Expression);
            Assert.Equal(2, fnExpression.Parameters.Count);

            var paramExpr = AssertAndCast<Identifier>(fnExpression.Parameters[0]);
            Assert.Equal("x", paramExpr.Value);
            paramExpr = AssertAndCast<Identifier>(fnExpression.Parameters[1]);
            Assert.Equal("y", paramExpr.Value);

            Assert.Equal(1, fnExpression.Body.Statements.Count);
            var body = AssertAndCast<ExpressionStatement>(fnExpression.Body.Statements[0]);
            var bodyExpr = AssertAndCast<InfixExpression>(body.Expression);

            Assert.Equal("x", bodyExpr.Left.TokenLiteral);
            Assert.Equal("+", bodyExpr.Operator);
            Assert.Equal("y", bodyExpr.Right.TokenLiteral);
        }

        [Fact]
        public void Parser_CanParseCallExpressions()
        {
            var input = @"
    add(1, 2 * 3, 4 + 5);
";

            AST result = subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var callExpression = AssertAndCast<CallExpression>(actual.Expression);
            Assert.Equal("add(1, (2 * 3), (4 + 5))", callExpression.StringValue);
        }

        [Fact]
        public void Parser_CanParseIfElseExpressions()
        {
            var input = @"
if (x < y) { x } else { y }
";

            AST result = subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            var actual = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var ifExpression = AssertAndCast<IfExpression>(actual.Expression);
            Assert.Equal("(x < y)", ifExpression.Condition.StringValue);

            Assert.Equal(1, ifExpression.Consequence.Statements.Count);
            var expr = AssertAndCast<ExpressionStatement>(ifExpression.Consequence.Statements[0]);
            var identifier = AssertAndCast<Identifier>(expr.Expression);
            Assert.Equal("x", identifier.Value);

            Assert.NotNull(ifExpression.Alternative);
            Assert.Equal(1, ifExpression.Alternative.Statements.Count);
            expr = AssertAndCast<ExpressionStatement>(ifExpression.Alternative.Statements[0]);
            identifier = AssertAndCast<Identifier>(expr.Expression);
            Assert.Equal("y", identifier.Value);
        }

        [Theory]
        [MemberData(nameof(PrefixExpressionData))]
        public void Parser_CanParsePrefixExpressions(string input, string expectedOperator, int expectedValue)
        {
            AST result = subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            var stmt = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var exp = AssertAndCast<PrefixExpression>(stmt.Expression);
            Assert.Equal(expectedOperator, exp.Operator);
            
            var intExpression = AssertAndCast<IntegerLiteral>(exp.Right);
            Assert.Equal(expectedValue, intExpression.Value);
        }

        public static IEnumerable<object[]> PrefixExpressionData =>
            new List<object[]>
            {
                new object[] { "!5;", "!", 5 },
                new object[] { "-15;", "-", 15 }
            };

        [Theory]
        [MemberData(nameof(InfixExpressionData))]
        public void Parser_CanParseInfixExpressions(string input, int expectedLeft, string expectedOp, int expectedRight)
        {
            AST result = subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            var stmt = AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            var exp = AssertAndCast<InfixExpression>(stmt.Expression);
            Assert.Equal(expectedOp, exp.Operator);

            var intExpression = AssertAndCast<IntegerLiteral>(exp.Left);
            Assert.Equal(expectedLeft, intExpression.Value);

            intExpression = AssertAndCast<IntegerLiteral>(exp.Right);
            Assert.Equal(expectedRight, intExpression.Value);
        }

        public static IEnumerable<object[]> InfixExpressionData =>
            new List<object[]>
            {
                new object[] { "5 + 6", 5, "+", 6 },
                new object[] { "5 - 6", 5, "-", 6 },
                new object[] { "5 * 6", 5, "*", 6 },
                new object[] { "5 / 6", 5, "/", 6 },
                new object[] { "5 > 6", 5, ">", 6 },
                new object[] { "5 < 6", 5, "<", 6 },
                new object[] { "5 == 6", 5, "==", 6 },
                new object[] { "5 != 6", 5, "!=", 6 },
            };

        [Theory]
        [MemberData(nameof(OperatorPrecendenceData))]
        public void Parser_CanHandleOperatorPrecendence(string input, string expected)
        {
            AST result = subject.ParseProgram(input);

            Assert.Empty(result.Errors);

            Assert.Equal(expected, result.Program.StringValue);
        }

        public static IEnumerable<object[]> OperatorPrecendenceData =>
            new List<object[]>
            {
                new object[] { "-a * b", "((-a) * b)" },
                new object[] { "!-a", "(!(-a))" },
                new object[] { "a + b + c", "((a + b) + c)" },
                new object[] { "a + b - c", "((a + b) - c)" },
                new object[] { "a + b / c", "(a + (b / c))" },
                new object[] { "a + b * c + d / e - f", "(((a + (b * c)) + (d / e)) - f)" },
                new object[] { "3 + 4; -5 * 5", "(3 + 4)((-5) * 5)" },
                new object[] { "5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))" },
                new object[] { "5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))" },
                new object[] { "3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))" },
                new object[] { "true", "true" },
                new object[] { "false", "false" },
                new object[] { "3 > 5 == false", "((3 > 5) == false)"},
                new object[] { "3 < 5 == true", "((3 < 5) == true)"}, 
                // Grouped expressions
                new object[] { "1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)" },
                new object[] { "(5 + 5) * 2", "((5 + 5) * 2)" },
                new object[] { "2 / (5 + 5)", "(2 / (5 + 5))" },
                new object[] { "-(5 + 5)", "(-(5 + 5))" },
                new object[] { "!(true == true)", "(!(true == true))" },
            };

        private T AssertAndCast<T>(object obj) where T : class
        {
            Assert.IsType<T>(obj);
            return obj as T;
        }

    }
}
