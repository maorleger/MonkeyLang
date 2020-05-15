using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class IndexExpression : IExpression
    {
        public IndexExpression(Token token, IExpression left, IExpression index)
        {
            Token = token;
            Left = left;
            Index = index;
        }

        public Token Token { get; }
        public IExpression Left { get; }
        public IExpression Index { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => $"({Left.StringValue}[{Index.StringValue}])";
    }
}
