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
            this.Token = token;
            this.Elements = elements.ToImmutableList();
        }

        public Token Token { get; }
        public IImmutableList<IExpression> Elements { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue
        {
            get
            {
                var sb = new StringBuilder("[");
                sb.AppendJoin(", ", this.Elements.Select(e => e.StringValue));
                sb.Append("]");
                return sb.ToString();
            }
        }
    }
}
