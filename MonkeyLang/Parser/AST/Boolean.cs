using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class Boolean : IExpression
    {
        public Boolean(Token token, bool value)
        {
            Token = token;
            Value = value;
        }

        public Token Token { get; }
        public bool Value { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => TokenLiteral;
    }
}
