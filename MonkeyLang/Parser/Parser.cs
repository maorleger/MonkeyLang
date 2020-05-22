using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

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
                { TokenType.Ident, () => new Identifier(this.CurrentToken, this.CurrentToken.Literal) },
                { TokenType.True, () => new Boolean(this.CurrentToken, true) },
                { TokenType.False, () => new Boolean(this.CurrentToken, false) },
                { TokenType.Int, () => new IntegerLiteral(this.CurrentToken, int.Parse(this.CurrentToken.Literal)) },
                { TokenType.String, () => new StringLiteral(this.CurrentToken, this.CurrentToken.Literal) },
                { TokenType.Bang, this.ParsePrefixExpression },
                { TokenType.Minus, this.ParsePrefixExpression },
                { TokenType.LParen, this.ParseGroupedExpression },
                { TokenType.If, this.ParseIfExpression },
                { TokenType.Function, this.ParseFunctionLiteral },
                { TokenType.LBracket, this.ParseArrayLiteral },
                { TokenType.LBrace, this.ParseHashLiteral }
            };

            this.InfixParseFns = new Dictionary<TokenType, Func<IExpression, IExpression>>()
            {
                { TokenType.Plus, this.ParseInfixExpression },
                { TokenType.Minus, this.ParseInfixExpression },
                { TokenType.Slash, this.ParseInfixExpression },
                { TokenType.Asterisk, this.ParseInfixExpression },
                { TokenType.Eq, this.ParseInfixExpression },
                { TokenType.Not_Eq, this.ParseInfixExpression },
                { TokenType.LT, this.ParseInfixExpression },
                { TokenType.GT, this.ParseInfixExpression },
                { TokenType.LParen, this.ParseCallExpression },
                { TokenType.LBracket, this.ParseIndexExpression }
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
            { TokenType.LParen, Precedence.Call },
            { TokenType.LBracket, Precedence.Index }
        };

        private Precedence PeekPrecedence() => this.Precedences.GetValueOrDefault(this.PeekToken.Type, Precedence.Lowest);

        private Precedence CurrentPrecedence() => this.Precedences.GetValueOrDefault(this.CurrentToken.Type, Precedence.Lowest);

        private void AdvanceTokens()
        {
            this.CurrentToken = this.PeekToken;
            this.PeekToken = this.Lexer.NextToken();
        }

        public AST ParseProgram(string input)
        {
            var statements = new List<IStatement>();
            var errors = new List<ParseException>();

            this.Lexer.Tokenize(input);
            this.AdvanceTokens();
            this.AdvanceTokens();

            while (this.CurrentToken.Type != TokenType.EOF)
            {
                try
                {
                    statements.Add(this.ParseStatement());
                }
                catch (ParseException e)
                {
                    errors.Add(e); // only add one parse error per statement
                    this.AdvanceToSemicolon();
                }
                this.AdvanceTokens();
            }

            return new AST(new Program(statements), errors);
        }

        private IStatement ParseStatement()
        {
            return this.CurrentToken.Type switch
            {
                TokenType.Let => this.ParseLetStatement(),
                TokenType.Return => this.ParseReturnStatement(),
                _ => this.ParseExpressionStatement()
            };
        }

        private IStatement ParseReturnStatement()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN RETURN");

            Token returnToken = this.CurrentToken;

            this.AdvanceTokens();

            IExpression? returnValue = this.ParseExpression(Precedence.Lowest);

            this.AdvanceToSemicolon();

            Trace.WriteLine("END RETURN");
            Trace.Unindent();

            return new ReturnStatement(returnToken, returnValue);
        }

        private IStatement ParseLetStatement()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN LET");
            Token letToken = this.CurrentToken;

            if (!this.ExpectPeek(TokenType.Ident))
            {
                throw new ParseException($"Expected an identifier, got {this.PeekToken}");
            }

            var name = new Identifier(this.CurrentToken, this.CurrentToken.Literal);

            if (!this.ExpectPeek(TokenType.Assign))
            {
                throw new ParseException($"Expected an assignment, got {this.PeekToken}");
            }

            this.AdvanceTokens();

            IExpression? letValue = this.ParseExpression(Precedence.Lowest);

            this.AdvanceToSemicolon();

            Trace.WriteLine("END LET");
            Trace.Unindent();
            return new LetStatement(letToken, name, letValue);

        }

        private IStatement ParseExpressionStatement()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN EXPRESSION_STATEMENT");
            Token expressionToken = this.CurrentToken;

            IExpression? expression = this.ParseExpression(Precedence.Lowest);

            this.ExpectPeek(TokenType.Semicolon); // Throw away the result

            Trace.WriteLine("END EXPRESSION_STATEMENT");
            Trace.Unindent();
            return new ExpressionStatement(expressionToken, expression);
        }

        private IExpression ParseExpression(Precedence precendence)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN EXPRESSION");
            this.PrefixParseFns.TryGetValue(this.CurrentToken.Type, out Func<IExpression>? prefix);
            if (prefix == null)
            {
                throw new ParseException($"no prefix parse function for {this.CurrentToken}");
            }
            IExpression? leftExpression = prefix();

            while (this.PeekToken.Type != TokenType.Semicolon && precendence < this.PeekPrecedence())
            {
                if (!this.InfixParseFns.ContainsKey(this.PeekToken.Type))
                {
                    return leftExpression;
                }

                this.AdvanceTokens();

                leftExpression = this.InfixParseFns[this.CurrentToken.Type](leftExpression);
            }

            Trace.WriteLine("END EXPRESSION");
            Trace.Unindent();
            return leftExpression;
        }

        private IExpression ParsePrefixExpression()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN PREFIX");
            Token token = this.CurrentToken;

            this.AdvanceTokens();

            IExpression? right = this.ParseExpression(Precedence.Prefix);

            Trace.WriteLine("END PREFIX");
            Trace.Unindent();
            return new PrefixExpression(token, token.Type, right);
        }

        private IExpression ParseInfixExpression(IExpression left)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN INFIX");

            Token token = this.CurrentToken;
            Precedence precedence = this.CurrentPrecedence();

            this.AdvanceTokens();

            IExpression? right = this.ParseExpression(precedence);

            Trace.WriteLine("END INFIX");
            Trace.Unindent();
            return new InfixExpression(token, left, token.Type, right);
        }

        private IExpression ParseCallExpression(IExpression function)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN CALL_EXPRESSION");

            Token token = this.CurrentToken;
            IEnumerable<IExpression>? arguments = this.ParseExpressionList(TokenType.RParen);

            Trace.WriteLine("END CALL_EXPRESSION");
            Trace.Unindent();

            return new CallExpression(token, function, arguments);
        }

        private IExpression ParseIndexExpression(IExpression left)
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN INDEX_EXPRESSION");

            Token token = this.CurrentToken;

            this.AdvanceTokens();

            IExpression? index = this.ParseExpression(Precedence.Lowest);

            if (!this.ExpectPeek(TokenType.RBracket))
            {
                throw new ParseException($"expected {TokenType.RBracket.GetDescription()}, got {this.PeekToken}");
            }

            Trace.WriteLine("END INDEX_EXPRESSION");
            Trace.Unindent();

            return new IndexExpression(token, left, index);
        }

        private IEnumerable<IExpression> ParseExpressionList(TokenType end)
        {
            var result = new List<IExpression>();

            this.AdvanceTokens();

            if (this.CurrentToken.Type == end)
            {
                return result;
            }

            result.Add(this.ParseExpression(Precedence.Lowest));

            while (this.PeekToken.Type == TokenType.Comma)
            {
                this.AdvanceTokens();
                this.AdvanceTokens();
                result.Add(this.ParseExpression(Precedence.Lowest));
            }

            if (!this.ExpectPeek(end))
            {
                throw new ParseException($"Expected {end.GetDescription()}, got {this.PeekToken}");
            }

            return result;
        }

        private IExpression ParseFunctionLiteral()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN FUNCTION");

            Token token = this.CurrentToken;

            this.ExpectPeek(TokenType.LParen);

            IEnumerable<Identifier>? parameters = this.ParseFunctionParameters();

            this.ExpectPeek(TokenType.LBrace);

            BlockStatement? body = this.ParseBlockStatement();

            Trace.WriteLine("END FUNCTION");
            Trace.Unindent();

            return new FunctionLiteral(token, parameters, body);
        }

        private IEnumerable<Identifier> ParseFunctionParameters()
        {
            var result = new List<Identifier>();

            this.AdvanceTokens();

            // No parameters
            if (this.CurrentToken.Type == TokenType.RParen)
            {
                return result;
            }

            result.Add(new Identifier(this.CurrentToken, this.CurrentToken.Literal));

            while (this.PeekToken.Type == TokenType.Comma)
            {
                this.AdvanceTokens();
                this.AdvanceTokens();
                result.Add(new Identifier(this.CurrentToken, this.CurrentToken.Literal));
            }

            this.ExpectPeek(TokenType.RParen);

            return result;
        }

        private IExpression ParseArrayLiteral()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN ARRAY");

            Token currentToken = this.CurrentToken;

            IEnumerable<IExpression> elements = this.ParseExpressionList(TokenType.RBracket);

            Trace.WriteLine("END ARRAY");
            Trace.Unindent();

            return new ArrayLiteral(currentToken, elements);
        }

        private IExpression ParseHashLiteral()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN HASH");

            Token currentToken = this.CurrentToken;
            var pairs = new Dictionary<IExpression, IExpression>();

            while (this.PeekToken.Type != TokenType.RBrace)
            {
                this.AdvanceTokens();

                IExpression? key = this.ParseExpression(Precedence.Lowest);

                if (!this.ExpectPeek(TokenType.Colon))
                {
                    throw new ParseException($"Expected ':', got {this.PeekToken}");
                }

                this.AdvanceTokens();

                IExpression? value = this.ParseExpression(Precedence.Lowest);

                pairs[key] = value;

                if (this.PeekToken.Type != TokenType.RBrace && !this.ExpectPeek(TokenType.Comma))
                {
                    throw new ParseException($"Expected '}}' or ',', got {this.PeekToken}");
                }
            }

            if (!this.ExpectPeek(TokenType.RBrace))
            {
                throw new ParseException($"Expected '}}', got {this.PeekToken}");
            }

            Trace.WriteLine("END HASH");
            Trace.Unindent();

            return new HashLiteral(currentToken, pairs);
        }

        private IExpression ParseGroupedExpression()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN GROUP");

            this.AdvanceTokens();

            IExpression? expression = this.ParseExpression(Precedence.Lowest);

            if (!this.ExpectPeek(TokenType.RParen))
            {
                throw new ParseException($"Expected right parens, got {this.PeekToken.Type}");
            }

            Trace.WriteLine("END GROUP");
            Trace.Unindent();
            return expression;
        }

        private IExpression ParseIfExpression()
        {
            Trace.Indent();
            Trace.WriteLine("BEGIN IF");

            Token currentToken = this.CurrentToken;

            if (!this.ExpectPeek(TokenType.LParen))
            {
                throw new ParseException($"Expected left parens, got {this.PeekToken.Type}");
            }

            IExpression? condition = this.ParseExpression(Precedence.Lowest);

            if (!this.ExpectPeek(TokenType.LBrace))
            {
                throw new ParseException($"Expected left brace, got {this.PeekToken.Type}");
            }

            BlockStatement? consequence = this.ParseBlockStatement();
            BlockStatement? alternative = null;

            if (this.PeekToken.Type == TokenType.Else)
            {
                this.AdvanceTokens();

                if (!this.ExpectPeek(TokenType.LBrace))
                {
                    throw new ParseException($"Expected left brace, got {this.PeekToken.Type}");
                }

                alternative = this.ParseBlockStatement();
            }

            Trace.WriteLine("END IF");
            Trace.Unindent();

            return new IfExpression(currentToken, condition, consequence, alternative);
        }

        private BlockStatement ParseBlockStatement()
        {
            Token currentToken = this.CurrentToken;
            var statements = new List<IStatement>();

            this.AdvanceTokens();

            while (this.CurrentToken.Type != TokenType.RBrace && this.CurrentToken.Type != TokenType.EOF)
            {
                statements.Add(this.ParseStatement());
                this.AdvanceTokens();
            }

            return new BlockStatement(currentToken, statements);
        }

        private void AdvanceToSemicolon()
        {
            while (this.CurrentToken.Type != TokenType.Semicolon && this.CurrentToken.Type != TokenType.EOF)
            {
                this.AdvanceTokens();
            }
        }

        private bool ExpectPeek(TokenType expectedType)
        {
            if (this.PeekToken.Type == expectedType)
            {
                this.AdvanceTokens();
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
            Call,
            Index
        }

    }
}
