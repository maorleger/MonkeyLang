using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace MonkeyLang
{
    public class HashLiteral : IExpression
    {
        public HashLiteral(Token token, IReadOnlyDictionary<IExpression, IExpression> pairs)
        {
            Token = token;
            Pairs = pairs.ToImmutableDictionary();
        }

        public Token Token { get; }
        public IImmutableDictionary<IExpression, IExpression> Pairs { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue
        {
            get
            {
                var sb = new StringBuilder("{");
                sb.AppendJoin(", ", Pairs.Select(kv => $"{kv.Key.StringValue}:{kv.Value.StringValue}"));
                sb.Append("}");
                return sb.ToString();
            }
        }
    }
}
