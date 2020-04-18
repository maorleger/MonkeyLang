using System;
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
            Token = token;
            Parameters = parameters.ToImmutableList();
            Body = body;
        }

        public Token Token { get; }
        public IImmutableList<Identifier> Parameters { get; }
        public BlockStatement Body { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue
        {
            get
            {
                StringBuilder result = new StringBuilder(TokenLiteral);
                result.Append("(");
                result.AppendJoin(", ", Parameters.Select(p => p.StringValue));
                result.Append(") ");
                result.Append(Body.StringValue);
                return result.ToString();
            }
        }
    }
}
