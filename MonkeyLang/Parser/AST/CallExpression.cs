using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MonkeyLang
{
    public class CallExpression : IExpression
    {
        public CallExpression(Token token, IExpression function, IEnumerable<IExpression> arguments)
        {
            Token = token;
            Function = function;
            Arguments = arguments.ToImmutableList();
        }

        public Token Token { get; }
        public IExpression Function { get; }
        public IImmutableList<IExpression> Arguments { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue
        {
            get
            {
                StringBuilder result = new StringBuilder(Function.StringValue);
                result.Append("(");
                result.AppendJoin(", ", Arguments.Select(a => a.StringValue));
                result.Append(")");

                return result.ToString();
            }
        }
    }
}
