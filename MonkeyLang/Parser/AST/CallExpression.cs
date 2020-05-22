using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class CallExpression : IExpression
    {
        public CallExpression(Token token, IExpression function, IEnumerable<IExpression> arguments)
        {
            this.Token = token;
            this.Function = function;
            this.Arguments = arguments.ToImmutableList();
        }

        public Token Token { get; }
        public IExpression Function { get; }
        public IImmutableList<IExpression> Arguments { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue
        {
            get
            {
                var result = new StringBuilder(this.Function.StringValue);
                result.Append("(");
                result.AppendJoin(", ", this.Arguments.Select(a => a.StringValue));
                result.Append(")");

                return result.ToString();
            }
        }
    }
}
