﻿namespace MonkeyLang
{
    public class Identifier : IExpression
    {
        public Identifier(Token token, string value)
        {
            this.Token = token;
            this.Value = value;
        }

        public Token Token { get; }
        public string Value { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => Value;
    }
}
