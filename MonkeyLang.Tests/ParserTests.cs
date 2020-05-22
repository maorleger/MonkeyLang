using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace MonkeyLang.Tests
{
    public class ParserTests
    {
        public ParserTests()
        {
            this.subject = new Parser(new Lexer());
        }

        private readonly Parser subject;

        [Fact]
        public void Parser_CanParseLetStatements()
        {
            string input = @"
    let x = 5 + 6;
    let y = 10;
    let foobar = hello;
";

            Program actual = this.subject.ParseProgram(input).Program;
            Assert.NotNull(actual.Statements);
            Assert.Equal(3, actual.Statements.Count());

            string[] expected = new[]
            {
                "x",
                "y",
                "foobar"
            };

            for (int i = 0; i < actual.Statements.Count(); i++)
            {
                IStatement element = actual.Statements.ElementAt(i);
                Assert.IsType<LetStatement>(element);
                Assert.Equal(expected[i], ((LetStatement)element).Name.TokenLiteral);
                // TODO: test for the RHS
            }
        }

        [Fact]
        public void Parser_CanListParseErrors()
        {
            string input = @"
let x 5;
let y = 10;
let = 10;
let 838383;
";

            AST actual = this.subject.ParseProgram(input);

            Assert.NotEmpty(actual.Errors);
            Assert.Equal(1, actual.Program.Statements.Count);
        }

        [Fact]
        public void Parser_CanParseReturnStatements()
        {
            string input = @"
return 5;
return fn(x,y) { x + y };
return xyz;
";

            AST actual = this.subject.ParseProgram(input);

            Assert.Equal(3, actual.Program.Statements.Count);
            Assert.Empty(actual.Errors);
            Assert.All(actual.Program.Statements, stmt => Assert.IsType<ReturnStatement>(stmt));
            // TODO: test for the RHS
        }

        [Fact]
        public void Parser_CanParseIdentifiers()
        {
            string input = @"
foobar;
";
            AST result = this.subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);

            Assert.IsType<Identifier>(actual.Expression);
            Assert.Equal("foobar", actual.TokenLiteral);

        }

        [Fact]
        public void Parser_CanParseIntegerLiterals()
        {
            string input = @"
5;
";
            AST result = this.subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            IntegerLiteral intExpression = this.AssertAndCast<IntegerLiteral>(actual.Expression);
            Assert.Equal(5, intExpression.Value);

            Assert.Equal("5", actual.TokenLiteral);
        }

        [Fact]
        public void Parser_CanParseBooleans()
        {
            string input = @"
true;
false
";

            AST result = this.subject.ParseProgram(input);

            Assert.Equal(2, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            Boolean boolExpression = this.AssertAndCast<Boolean>(actual.Expression);
            Assert.True(boolExpression.Value);

            actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[1]);
            boolExpression = this.AssertAndCast<Boolean>(actual.Expression);
            Assert.False(boolExpression.Value);
        }

        [Fact]
        public void Parser_CanParseIfExpressions()
        {
            string input = @"
if (x < y) { x }
";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            IfExpression ifExpression = this.AssertAndCast<IfExpression>(actual.Expression);
            Assert.Equal("(x < y)", ifExpression.Condition.StringValue);

            Assert.Equal(1, ifExpression.Consequence.Statements.Count);
            ExpressionStatement consequenceExpression = this.AssertAndCast<ExpressionStatement>(ifExpression.Consequence.Statements[0]);
            Identifier consequenceIdentifier = this.AssertAndCast<Identifier>(consequenceExpression.Expression);
            Assert.Equal("x", consequenceIdentifier.Value);

            Assert.Null(ifExpression.Alternative);
        }

        [Fact]
        public void Parser_CanParseFunctionLiteral()
        {
            string input = @"
fn(x, y) { x + y; }
";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            FunctionLiteral fnExpression = this.AssertAndCast<FunctionLiteral>(actual.Expression);
            Assert.Equal(2, fnExpression.Parameters.Count);

            Identifier paramExpr = this.AssertAndCast<Identifier>(fnExpression.Parameters[0]);
            Assert.Equal("x", paramExpr.Value);
            paramExpr = this.AssertAndCast<Identifier>(fnExpression.Parameters[1]);
            Assert.Equal("y", paramExpr.Value);

            Assert.Equal(1, fnExpression.Body.Statements.Count);
            ExpressionStatement body = this.AssertAndCast<ExpressionStatement>(fnExpression.Body.Statements[0]);
            InfixExpression bodyExpr = this.AssertAndCast<InfixExpression>(body.Expression);

            Assert.Equal("x", bodyExpr.Left.TokenLiteral);
            Assert.Equal(TokenType.Plus, bodyExpr.Operator);
            Assert.Equal("y", bodyExpr.Right.TokenLiteral);
        }

        [Fact]
        public void Parser_CanParseCallExpressions()
        {
            string input = @"
    add(1, 2 * 3, 4 + 5);
";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            CallExpression callExpression = this.AssertAndCast<CallExpression>(actual.Expression);
            Assert.Equal("add(1, (2 * 3), (4 + 5))", callExpression.StringValue);
        }

        [Fact]
        public void Parser_CanParseIfElseExpressions()
        {
            string input = @"
if (x < y) { x } else { y }
";

            AST result = this.subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            ExpressionStatement actual = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            IfExpression ifExpression = this.AssertAndCast<IfExpression>(actual.Expression);
            Assert.Equal("(x < y)", ifExpression.Condition.StringValue);

            Assert.Equal(1, ifExpression.Consequence.Statements.Count);
            ExpressionStatement expr = this.AssertAndCast<ExpressionStatement>(ifExpression.Consequence.Statements[0]);
            Identifier identifier = this.AssertAndCast<Identifier>(expr.Expression);
            Assert.Equal("x", identifier.Value);

            Assert.NotNull(ifExpression.Alternative);
            Assert.Equal(1, ifExpression.Alternative.Statements.Count);
            expr = this.AssertAndCast<ExpressionStatement>(ifExpression.Alternative.Statements[0]);
            identifier = this.AssertAndCast<Identifier>(expr.Expression);
            Assert.Equal("y", identifier.Value);
        }

        [Theory]
        [MemberData(nameof(PrefixExpressionData))]
        public void Parser_CanParsePrefixExpressions(string input, TokenType expectedOperator, int expectedValue)
        {
            AST result = this.subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            ExpressionStatement stmt = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            PrefixExpression exp = this.AssertAndCast<PrefixExpression>(stmt.Expression);
            Assert.Equal(expectedOperator, exp.Operator);

            IntegerLiteral intExpression = this.AssertAndCast<IntegerLiteral>(exp.Right);
            Assert.Equal(expectedValue, intExpression.Value);
        }

        public static IEnumerable<object[]> PrefixExpressionData =>
            new List<object[]>
            {
                new object[] { "!5;", TokenType.Bang, 5 },
                new object[] { "-15;", TokenType.Minus, 15 }
            };

        [Theory]
        [MemberData(nameof(InfixExpressionData))]
        public void Parser_CanParseInfixExpressions(string input, int expectedLeft, TokenType expectedOp, int expectedRight)
        {
            AST result = this.subject.ParseProgram(input);

            Assert.Equal(1, result.Program.Statements.Count);
            Assert.Empty(result.Errors);

            ExpressionStatement stmt = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            InfixExpression exp = this.AssertAndCast<InfixExpression>(stmt.Expression);
            Assert.Equal(expectedOp, exp.Operator);

            IntegerLiteral intExpression = this.AssertAndCast<IntegerLiteral>(exp.Left);
            Assert.Equal(expectedLeft, intExpression.Value);

            intExpression = this.AssertAndCast<IntegerLiteral>(exp.Right);
            Assert.Equal(expectedRight, intExpression.Value);
        }

        public static IEnumerable<object[]> InfixExpressionData =>
            new List<object[]>
            {
                new object[] { "5 + 6", 5, TokenType.Plus, 6 },
                new object[] { "5 - 6", 5, TokenType.Minus, 6 },
                new object[] { "5 * 6", 5, TokenType.Asterisk, 6 },
                new object[] { "5 / 6", 5, TokenType.Slash, 6 },
                new object[] { "5 > 6", 5, TokenType.GT, 6 },
                new object[] { "5 < 6", 5, TokenType.LT, 6 },
                new object[] { "5 == 6", 5, TokenType.Eq, 6 },
                new object[] { "5 != 6", 5, TokenType.Not_Eq, 6 },
            };

        [Theory]
        [MemberData(nameof(OperatorPrecendenceData))]
        public void Parser_CanHandleOperatorPrecendence(string input, string expected)
        {
            AST result = this.subject.ParseProgram(input);

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
                //Array indexing
                new object[] { "a * [1, 2, 3, 4][b * c] * d", "((a * ([1, 2, 3, 4][(b * c)])) * d)", },
                new object[] { "add(a * b[2], b[1], 2 * [1, 2][1])", "add((a * (b[2])), (b[1]), (2 * ([1, 2][1])))", },
            };

        [Fact]
        public void Parser_CanParseStringLitrals()
        {
            string input = @"""hello world"";";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);
            ExpressionStatement expr = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            StringLiteral stringResult = this.AssertAndCast<StringLiteral>(expr.Expression);

            Assert.Equal("hello world", stringResult.Value);
        }

        [Fact]
        public void Parser_CanParseArrayLiterals()
        {
            string input = @"[1, 2 * 2, 3 + 3]";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);
            ExpressionStatement expr = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            ArrayLiteral arrayResult = this.AssertAndCast<ArrayLiteral>(expr.Expression);
            Assert.Equal(3, arrayResult.Elements.Count);

            IntegerLiteral intLiteral = this.AssertAndCast<IntegerLiteral>(arrayResult.Elements[0]);
            Assert.Equal(1, intLiteral.Value);

            InfixExpression exprLiteral = this.AssertAndCast<InfixExpression>(arrayResult.Elements[1]);
            Assert.Equal("(2 * 2)", exprLiteral.StringValue);

            exprLiteral = this.AssertAndCast<InfixExpression>(arrayResult.Elements[2]);
            Assert.Equal("(3 + 3)", exprLiteral.StringValue);
        }

        [Fact]
        public void Parser_CanParseIndexExpressions()
        {
            string input = "myArray[1 + 1]";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);
            ExpressionStatement expr = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            IndexExpression indexExpr = this.AssertAndCast<IndexExpression>(expr.Expression);

            string ident = (indexExpr.Left as Identifier).Value;
            Assert.Equal("myArray", ident);

            this.Parser_CanParseInfixExpressions(indexExpr.Index.StringValue, 1, TokenType.Plus, 1);
        }

        [Fact]
        public void Parser_CanParseHashLiterals()
        {
            string input = @"{""one"":1, ""two"": 2, ""three"": 3}";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);
            ExpressionStatement expr = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            HashLiteral hashExpr = this.AssertAndCast<HashLiteral>(expr.Expression);

            Assert.Equal(3, hashExpr.Pairs.Count);

            var expected = new Dictionary<string, string>()
            {
                { "one", "1" },
                { "two", "2" },
                { "three", "3" }
            }.ToImmutableDictionary();

            var actual = ImmutableDictionary.CreateRange<string, string>(
                hashExpr.Pairs.Select(kv =>
                    new KeyValuePair<string, string>(kv.Key.StringValue, kv.Value.StringValue)
                )
            );
        }

        [Fact]
        public void Parser_CanParseEmptyHashes()
        {
            string input = "{}";

            AST result = this.subject.ParseProgram(input);

            Assert.Empty(result.Errors);
            Assert.Equal(1, result.Program.Statements.Count);
            ExpressionStatement expr = this.AssertAndCast<ExpressionStatement>(result.Program.Statements[0]);
            HashLiteral hashExpr = this.AssertAndCast<HashLiteral>(expr.Expression);

            Assert.Equal(0, hashExpr.Pairs.Count);
        }

        private T AssertAndCast<T>(object obj) where T : class
        {
            Assert.IsType<T>(obj);
            return obj as T;
        }

    }
}
