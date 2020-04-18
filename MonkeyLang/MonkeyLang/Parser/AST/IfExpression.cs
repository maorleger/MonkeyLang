using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class IfExpression : IExpression
    {
        public IfExpression(Token token, IExpression condition, BlockStatement consequence, BlockStatement? alternative)
        {
            Token = token;
            Condition = condition;
            Consequence = consequence;
            Alternative = alternative;
        }

        public Token Token { get; }
        public IExpression Condition { get; }
        public BlockStatement Consequence { get; }
        public BlockStatement? Alternative { get; } // TODO: should this never be null?

        public string TokenLiteral => Token.Literal;

        public string StringValue
        {
            get
            {
                var result = $"if {Condition.StringValue} {Consequence.StringValue} ";
                if (Alternative != null)
                {
                    result += $"else {Alternative.StringValue}";
                }
                return result;
            }
        }
    }
}
