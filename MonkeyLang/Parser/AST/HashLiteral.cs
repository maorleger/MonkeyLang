using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class HashLiteral : IExpression
    {
        public HashLiteral(Token token, IReadOnlyDictionary<IExpression, IExpression> pairs)
        {
            this.Token = token;
            this.Pairs = pairs.ToImmutableDictionary();
        }

        public Token Token { get; }
        public IImmutableDictionary<IExpression, IExpression> Pairs { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue
        {
            get
            {
                var sb = new StringBuilder("{");
                sb.AppendJoin(", ", this.Pairs.Select(kv => $"{kv.Key.StringValue}:{kv.Value.StringValue}"));
                sb.Append("}");
                return sb.ToString();
            }
        }
    }
}
