using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class ArrayLiteral : IExpression
    {
        public ArrayLiteral(Token token, IEnumerable<IExpression> elements)
        {
            Token = token;
            Elements = elements.ToImmutableList();
        }

        public Token Token { get; }
        public IImmutableList<IExpression> Elements { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue
        {
            get
            {
                StringBuilder sb = new StringBuilder("[");
                sb.AppendJoin(", ", Elements.Select(e => e.StringValue));
                sb.Append("]");
                return sb.ToString();
            }
        }
    }
}
