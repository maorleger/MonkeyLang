﻿using Pidgin;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace MonkeyLang
{
    [Export(typeof(Lexer))]
    public class Lexer
    {
        public Lexer()
        {
            this.Tokens = new Queue<Token>();
        }

        public Token NextToken()
        {
            if (this.Tokens.TryDequeue(out Token result))
            {
                return result;
            }

            return new Token(TokenType.EOF, "");
        }

        public void Tokenize(string rawInput)
        {
            var result = Tok(Monkey).ParseOrThrow(rawInput);
            Tokens.EnqueueAll(result);
        }

        private Queue<Token> Tokens { get; }

        // Wrappers
        private static Parser<char, T> Tok<T>(Parser<char, T> token)
           => Try(token).Between(SkipWhitespaces);

        private static Parser<char, string> Tok(string token)
            => Try(String(token)).Between(SkipWhitespaces);

        // Basic symbols
        static readonly Parser<char, Token> Assign = Tok("=").Select(t => new Token(TokenType.Assign, t));
        static readonly Parser<char, Token> Plus = Tok("+").Select(t => new Token(TokenType.Plus, t));
        static readonly Parser<char, Token> LParen = Tok("(").Select(t => new Token(TokenType.LParen, t));
        static readonly Parser<char, Token> RParen = Tok(")").Select(t => new Token(TokenType.RParen, t));
        static readonly Parser<char, Token> LBrace = Tok("{").Select(t => new Token(TokenType.LBrace, t));
        static readonly Parser<char, Token> RBrace = Tok("}").Select(t => new Token(TokenType.RBrace, t));
        static readonly Parser<char, Token> Comma = Tok(",").Select(t => new Token(TokenType.Comma, t));
        static readonly Parser<char, Token> SemiColon = Tok(";").Select(t => new Token(TokenType.Semicolon, t));
        static readonly Parser<char, Token> Bang = Tok("!").Select(t => new Token(TokenType.Bang, t));
        static readonly Parser<char, Token> Minus = Tok("-").Select(t => new Token(TokenType.Minus, t));
        static readonly Parser<char, Token> Slash = Tok("/").Select(t => new Token(TokenType.Slash, t));
        static readonly Parser<char, Token> Asterisk = Tok("*").Select(t => new Token(TokenType.Asterisk, t));
        static readonly Parser<char, Token> LT = Tok("<").Select(t => new Token(TokenType.LT, t));
        static readonly Parser<char, Token> GT = Tok(">").Select(t => new Token(TokenType.GT, t));
        static readonly Parser<char, Token> Eq = Tok("==").Select(t => new Token(TokenType.Eq, t));
        static readonly Parser<char, Token> Not_Eq = Tok("!=").Select(t => new Token(TokenType.Not_Eq, t));
        static readonly Parser<char, Token> Symbol = OneOf(
            Eq,
            Not_Eq,
            Assign,
            Plus,
            LParen,
            RParen,
            LBrace,
            RBrace,
            Comma,
            SemiColon,
            Bang,
            Minus,
            Slash,
            Asterisk,
            LT,
            GT
        );

        // Keywords
        static readonly Parser<char, Token> Let = Tok("let").Select(t => new Token(TokenType.Let, t));
        static readonly Parser<char, Token> Fn = Tok("fn").Select(t => new Token(TokenType.Function, t));
        static readonly Parser<char, Token> If = Tok("if").Select(t => new Token(TokenType.If, t));
        static readonly Parser<char, Token> Else = Tok("else").Select(t => new Token(TokenType.Else, t));
        static readonly Parser<char, Token> True = Tok("true").Select(t => new Token(TokenType.True, t));
        static readonly Parser<char, Token> False = Tok("false").Select(t => new Token(TokenType.False, t));
        static readonly Parser<char, Token> Return = Tok("return").Select(t => new Token(TokenType.Return, t));
        static readonly Parser<char, Token> Keyword = OneOf(
            Let,
            Fn,
            If,
            Else,
            True,
            False,
            Return
         );

        // Identifiers
        static readonly Parser<char, Token> Integer =
            Tok(Digit.Then(Digit.ManyString(), (h, t) => h + t))
            .Select(t => new Token(TokenType.Int, t));

        static readonly Parser<char, char> ValidIdentifierChar = OneOf(Letter, Char('_'));
        static readonly Parser<char, Token> Name =
            Tok(ValidIdentifierChar.Then(ValidIdentifierChar.ManyString(), (h, t) => h + t))
            .Select(t => new Token(TokenType.Ident, t));

        static readonly Parser<char, Token> Identifier = OneOf(Integer, Name);

        // Illegal
        static readonly Parser<char, Token> Illegal = Tok(Any).Select(t => new Token(TokenType.Illegal, t.ToString()));

        // Top level parser
        static readonly Parser<char, IEnumerable<Token>> Monkey =
            OneOf(Symbol, Keyword, Identifier, Illegal).Many();
    }
}