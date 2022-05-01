using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class FunctionLiteral : IExpression
    {
        public FunctionLiteral(Token token, IEnumerable<Identifier> parameters, BlockStatement body)
        {
            this.Token = token;
            this.Parameters = parameters.ToImmutableList();
            this.Body = body;
        }

        public Token Token { get; }
        public IImmutableList<Identifier> Parameters { get; }
        public BlockStatement Body { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue
        {
            get
            {
                var result = new StringBuilder(this.TokenLiteral);
                result.Append("(");
                result.AppendJoin(", ", this.Parameters.Select(p => p.StringValue));
                result.Append(") ");
                result.Append(this.Body.StringValue);
                return result.ToString();
            }
        }
    }
}
