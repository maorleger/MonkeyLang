using System.Collections.Generic;
using MonkeyLang;
using Xunit;

namespace MonkeyLang.Tests
{
    public class LexerTests
    {
        private readonly Lexer subject;

        public LexerTests()
        {
            subject = new Lexer();
        }
        
        [Fact]
        public void Lexer_CanProcessMultipleItems()
        {
            subject.Tokenize("x");
            subject.Tokenize("y");
            subject.Tokenize("z");

            var expected = new List<Token>()
            {
                new Token(TokenType.Ident, "x"),
                new Token(TokenType.Ident, "y"),
                new Token(TokenType.Ident, "z"),
                new Token(TokenType.EOF, "")
            };

            foreach (var item in expected)
            {
                Assert.Equal(item, subject.NextToken());
            }
        }

        [Fact]
        public void Lexer_WhenEmpty_DoesNotFail()
        {
            var expected = new Token(TokenType.EOF, "");
            Assert.Equal(expected, subject.NextToken());
            Assert.Equal(expected, subject.NextToken());
            Assert.Equal(expected, subject.NextToken());
            Assert.Equal(expected, subject.NextToken());

        }

        [Fact]
        public void Lexer_CanParseMultipleLines()
        {
            subject.Tokenize(@"
let five_things_ = 5;
let ten = 10;

let add = fn(x, y) {
    x + y
};
@
let result = add(five, ten);
!-/*5;
5 < 10 > 5;
if (5 < 10) {
    return true;
} else {
    return false;
}
10 == 10;
10 != 9;
""foobar"";
""foo bar""
            ");

            var expected = new List<Token>()
            {
                new Token(TokenType.Let, "let"),
                new Token(TokenType.Ident, "five_things_"),
                new Token(TokenType.Assign, "="),
                new Token(TokenType.Int, "5"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Let, "let"),
                new Token(TokenType.Ident, "ten"),
                new Token(TokenType.Assign, "="),
                new Token(TokenType.Int, "10"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Let, "let"),
                new Token(TokenType.Ident, "add"),
                new Token(TokenType.Assign, "="),
                new Token(TokenType.Function, "fn"),
                new Token(TokenType.LParen, "("),
                new Token(TokenType.Ident, "x"),
                new Token(TokenType.Comma, ","),
                new Token(TokenType.Ident, "y"),
                new Token(TokenType.RParen, ")"),
                new Token(TokenType.LBrace, "{"),
                new Token(TokenType.Ident, "x"),
                new Token(TokenType.Plus, "+"),
                new Token(TokenType.Ident, "y"),
                new Token(TokenType.RBrace, "}"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Illegal, "@"),
                new Token(TokenType.Let, "let"),
                new Token(TokenType.Ident, "result"),
                new Token(TokenType.Assign, "="),
                new Token(TokenType.Ident, "add"),
                new Token(TokenType.LParen, "("),
                new Token(TokenType.Ident, "five"),
                new Token(TokenType.Comma, ","),
                new Token(TokenType.Ident, "ten"),
                new Token(TokenType.RParen, ")"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Bang, "!"),
                new Token(TokenType.Minus, "-"),
                new Token(TokenType.Slash, "/"),
                new Token(TokenType.Asterisk, "*"),
                new Token(TokenType.Int, "5"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Int, "5"),
                new Token(TokenType.LT, "<"),
                new Token(TokenType.Int, "10"),
                new Token(TokenType.GT, ">"),
                new Token(TokenType.Int, "5"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.If, "if"),
                new Token(TokenType.LParen, "("),
                new Token(TokenType.Int, "5"),
                new Token(TokenType.LT, "<"),
                new Token(TokenType.Int, "10"),
                new Token(TokenType.RParen, ")"),
                new Token(TokenType.LBrace, "{"),
                new Token(TokenType.Return, "return"),
                new Token(TokenType.True, "true"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.RBrace, "}"),
                new Token(TokenType.Else, "else"),
                new Token(TokenType.LBrace, "{"),
                new Token(TokenType.Return, "return"),
                new Token(TokenType.False, "false"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.RBrace, "}"),
                new Token(TokenType.Int, "10"),
                new Token(TokenType.Eq, "=="),
                new Token(TokenType.Int, "10"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Int, "10"),
                new Token(TokenType.Not_Eq, "!="),
                new Token(TokenType.Int, "9"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.String, "foobar"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.String, "foo bar"),
                new Token(TokenType.EOF, "")
            };

            foreach (var item in expected)
            {
                var actual = subject.NextToken();
                Assert.Equal(item, actual);
            }
        }

        [Theory]
        [MemberData(nameof(Statements))]
        public void Lexer_CanParseIndividualTokens(string input, Token expected)
        {
            subject.Tokenize(input);
            var actual = subject.NextToken();
            Assert.Equal(expected.Literal, actual.Literal);
            Assert.Equal(expected.Type, actual.Type);
        }

        public static IEnumerable<object[]> Statements =>
            new List<object[]>
            {
                new object[] { "=", new Token(TokenType.Assign, "=") },
                new object[] { "+", new Token(TokenType.Plus, "+") },
                new object[] { "(", new Token(TokenType.LParen, "(") },
                new object[] { ")", new Token(TokenType.RParen, ")") },
                new object[] { "{", new Token(TokenType.LBrace, "{") },
                new object[] { "}", new Token(TokenType.RBrace, "}") },
                new object[] { ",", new Token(TokenType.Comma, ",") },
                new object[] { ";", new Token(TokenType.Semicolon, ";") },
                new object[] { "" , new Token(TokenType.EOF, "") },
                new object[] { "let" , new Token(TokenType.Let, "let") },
                new object[] { "fn" , new Token(TokenType.Function, "fn") },
                new object[] { "ten" , new Token(TokenType.Ident, "ten") },
                new object[] { "ten_fold" , new Token(TokenType.Ident, "ten_fold") },
                new object[] { "10" , new Token(TokenType.Int, "10") },
                new object[] { "$" , new Token(TokenType.Illegal, "$") },
            };
    }
}
