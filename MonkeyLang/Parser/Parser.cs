using Pidgin;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MonkeyLang
{
    [Export(typeof(Parser))]
    public class Parser
    {
        [ImportingConstructor]
        public Parser([Import] Lexer lexer)
        {
            this.Lexer = lexer;
            this.PrefixParseFns = new Dictionary<TokenType, Func<IExpression>>()
            {
                { TokenType.Ident, () => new Identifier(CurrentToken, CurrentToken.Literal) },
                { TokenType.True, () => new Boolean(CurrentToken, true) },
                { TokenType.False, () => new Boolean(CurrentToken, false) },
                { TokenType.Int, () => new IntegerLiteral(CurrentToken, int.Parse(CurrentToken.Literal)) },
                { TokenType.Bang, ParsePrefixExpression },
                { TokenType.Minus, ParsePrefixExpression },
                { TokenType.LParen, ParseGroupedExpression },
                { TokenType.If, ParseIfExpression },
                { TokenType.Function, ParseFunctionLiteral }
            };

            this.InfixParseFns = new Dictionary<TokenType, Func<IExpression, IExpression>>()
            {
                { TokenType.Plus, ParseInfixExpression },
                { TokenType.Minus, ParseInfixExpression },
                { TokenType.Slash, ParseInfixExpression },
                { TokenType.Asterisk, ParseInfixExpression },
                { TokenType.Eq, ParseInfixExpression },
                { TokenType.Not_Eq, ParseInfixExpression },
                { TokenType.LT, ParseInfixExpression },
                { TokenType.GT, ParseInfixExpression },
                { TokenType.LParen, ParseCallExpression }
            };
        }

        private Lexer Lexer { get; }
        private Token CurrentToken { get; set; }
        private Token PeekToken { get; set; }

        private Dictionary<TokenType, Func<IExpression>> PrefixParseFns { get; }
        private readonly Dictionary<TokenType, Func<IExpression, IExpression>> InfixParseFns;
        private readonly Dictionary<TokenType, Precedence> Precedences = new Dictionary<TokenType, Precedence>()
        {
            { TokenType.Eq, Precedence.Equals },
            { TokenType.Not_Eq, Precedence.Equals },
            { TokenType.LT, Precedence.LessGreater },
            { TokenType.GT, Precedence.LessGreater },
            { TokenType.Plus, Precedence.Sum },
            { TokenType.Minus, Precedence.Sum },
            { TokenType.Slash, Precedence.Product },
            { TokenType.Asterisk, Precedence.Product },
            { TokenType.LParen, Precedence.Call }
        };

        private Precedence PeekPrecedence() => Precedences.GetValueOrDefault(PeekToken.Type, Precedence.Lowest);

        private Precedence CurrentPrecedence() => Precedences.GetValueOrDefault(CurrentToken.Type, Precedence.Lowest);

        private void AdvanceTokens()
        {
            this.CurrentToken = PeekToken;
            this.PeekToken = Lexer.NextToken();
        }

        public AST ParseProgram(string input)
        {
            var statements = new List<IStatement>();
            var errors = new List<MonkeyParseException>();

            this.Lexer.Tokenize(input);
            AdvanceTokens();
            AdvanceTokens();

            while (CurrentToken.Type != TokenType.EOF)
            {
                try
                {
                    statements.Add(ParseStatement());
            ;
                }
                catch (MonkeyParseException e)
                {
                    errors.Add(e); // only add one parse error per statement
                    AdvanceToSemicolon();
                }
                AdvanceTokens();
            }

            return new AST(new Program(statements), errors);
        }

        private IStatement ParseStatement()
        {
            return CurrentToken.Type switch
            {
                TokenType.Let => ParseLetStatement(),
                TokenType.Return => ParseReturnStatement(),
                _ => ParseExpressionStatement()
            };
        }

        private IStatement ParseReturnStatement()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN RETURN");

            var returnToken = CurrentToken;

            AdvanceTokens();

            var returnValue = ParseExpression(Precedence.Lowest);

            AdvanceToSemicolon();

            Trace.WriteLine("END RETURN");
            Trace.Unindent();

            return new ReturnStatement(returnToken, returnValue);
        }

        private IStatement ParseLetStatement()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN LET");
            var letToken = CurrentToken;

            if (!ExpectPeek(TokenType.Ident))
            {
                throw new MonkeyParseException($"Expected an identifier, got {PeekToken}");
            }

            Identifier name = new Identifier(CurrentToken, CurrentToken.Literal);

            if (!ExpectPeek(TokenType.Assign))
            {
                throw new MonkeyParseException($"Expected an assignment, got {PeekToken}");
            }

            AdvanceTokens();

            var letValue = ParseExpression(Precedence.Lowest);

            AdvanceToSemicolon();

            Trace.WriteLine("END LET");
            Trace.Unindent();
            return new LetStatement(letToken, name, letValue);

        }

        private IStatement ParseExpressionStatement()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN EXPRESSION_STATEMENT");
            var expressionToken = CurrentToken;

            var expression = ParseExpression(Precedence.Lowest);

            ExpectPeek(TokenType.Semicolon); // Throw away the result

            Trace.WriteLine("END EXPRESSION_STATEMENT");
            Trace.Unindent();
            return new ExpressionStatement(expressionToken, expression);
        }

        private IExpression ParseExpression(Precedence precendence)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN EXPRESSION");
            PrefixParseFns.TryGetValue(CurrentToken.Type, out var prefix);
            if (prefix == null)
            {
                throw new MonkeyParseException($"no prefix parse function for {CurrentToken}");
            }
            var leftExpression = prefix();

            while (PeekToken.Type != TokenType.Semicolon && precendence < PeekPrecedence())
            {
                if (!InfixParseFns.ContainsKey(PeekToken.Type))
                {
                    return leftExpression;
                }

                AdvanceTokens();

                leftExpression = InfixParseFns[CurrentToken.Type](leftExpression);
            }

            Trace.WriteLine("END EXPRESSION");
            Trace.Unindent();
            return leftExpression;
        }

        private IExpression ParsePrefixExpression()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN PREFIX");
            var token = CurrentToken;

            AdvanceTokens();

            var right = ParseExpression(Precedence.Prefix);

            Trace.WriteLine("END PREFIX");
            Trace.Unindent();
            return new PrefixExpression(token, token.Type, right);
        }

        private IExpression ParseInfixExpression(IExpression left)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN INFIX");

            var token = CurrentToken;
            var precedence = CurrentPrecedence();

            AdvanceTokens();

            var right = ParseExpression(precedence);

            Trace.WriteLine("END INFIX");
            Trace.Unindent();
            return new InfixExpression(token, left, token.Type, right);
        }
        
        private IExpression ParseCallExpression(IExpression function)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN CALL_EXPRESSION");

            var token = CurrentToken;
            var arguments = ParseCallArguments();

            Trace.WriteLine("END CALL_EXPRESSION");
            Trace.Unindent();

            return new CallExpression(token, function, arguments);
        }

        private IEnumerable<IExpression> ParseCallArguments()
        {
            List<IExpression> result = new List<IExpression>();

            AdvanceTokens();

            if (CurrentToken.Type == TokenType.RParen)
            {
                return result;
            }

            result.Add(ParseExpression(Precedence.Lowest));

            while (PeekToken.Type == TokenType.Comma)
            {
                AdvanceTokens();
                AdvanceTokens();
                result.Add(ParseExpression(Precedence.Lowest));
            }

            ExpectPeek(TokenType.RParen);

            return result;
        }

        private IExpression ParseFunctionLiteral()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN FUNCTION");

            var token = CurrentToken;

            ExpectPeek(TokenType.LParen);

            var parameters = ParseFunctionParameters();

            ExpectPeek(TokenType.LBrace);

            var body = ParseBlockStatement();

            Trace.WriteLine("END FUNCTION");
            Trace.Unindent();

            return new FunctionLiteral(token, parameters, body);
        }

        private IEnumerable<Identifier> ParseFunctionParameters()
        {
            List<Identifier> result = new List<Identifier>();

            AdvanceTokens();

            // No parameters
            if (CurrentToken.Type == TokenType.RParen)
            {
                return result;
            }

            result.Add(new Identifier(CurrentToken, CurrentToken.Literal));

            while (PeekToken.Type == TokenType.Comma)
            {
                AdvanceTokens();
                AdvanceTokens();
                result.Add(new Identifier(CurrentToken, CurrentToken.Literal));
            }

            ExpectPeek(TokenType.RParen);

            return result;
        }

        private IExpression ParseGroupedExpression()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN GROUP");

            AdvanceTokens();

            var expression = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.RParen))
            {
                throw new MonkeyParseException($"Expected right parens, got {PeekToken.Type}");
            }

            Trace.WriteLine("END GROUP");
            Trace.Unindent();
            return expression;
        }

        private IExpression ParseIfExpression()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN IF");

            var currentToken = CurrentToken;

            if (!ExpectPeek(TokenType.LParen))
            {
                throw new MonkeyParseException($"Expected left parens, got {PeekToken.Type}");
            }

            var condition = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.LBrace))
            {
                throw new MonkeyParseException($"Expected left brace, got {PeekToken.Type}");
            }

            var consequence = ParseBlockStatement();
            BlockStatement? alternative = null;

            if (PeekToken.Type == TokenType.Else)
            {
                AdvanceTokens();

                if (!ExpectPeek(TokenType.LBrace))
                {
                    throw new MonkeyParseException($"Expected left brace, got {PeekToken.Type}");
                }

                alternative = ParseBlockStatement();
            }

            Trace.WriteLine("END IF");
            Trace.Unindent();

            return new IfExpression(currentToken, condition, consequence, alternative);
        }

        private BlockStatement ParseBlockStatement()
        {
            var currentToken = CurrentToken;
            var statements = new List<IStatement>();

            AdvanceTokens();

            while (CurrentToken.Type != TokenType.RBrace && CurrentToken.Type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
                AdvanceTokens();
            }

            return new BlockStatement(currentToken, statements);
        }

        private void AdvanceToSemicolon()
        {
            while (CurrentToken.Type != TokenType.Semicolon && CurrentToken.Type != TokenType.EOF)
            {
                AdvanceTokens();
            }
        }

        private bool ExpectPeek(TokenType expectedType)
        {
            if (PeekToken.Type == expectedType)
            {
                AdvanceTokens();
                return true;
            }
            return false;
        }

        enum Precedence
        {
            Lowest,
            Equals,
            LessGreater,
            Sum,
            Product,
            Prefix,
            Call
        }

    }
}
