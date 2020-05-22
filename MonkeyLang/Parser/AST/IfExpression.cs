namespace MonkeyLang
{
    public class IfExpression : IExpression
    {
        public IfExpression(Token token, IExpression condition, BlockStatement consequence, BlockStatement? alternative)
        {
            this.Token = token;
            this.Condition = condition;
            this.Consequence = consequence;
            this.Alternative = alternative;
        }

        public Token Token { get; }
        public IExpression Condition { get; }
        public BlockStatement Consequence { get; }
        public BlockStatement? Alternative { get; } // TODO: should this never be null?

        public string TokenLiteral => this.Token.Literal;

        public string StringValue
        {
            get
            {
                string? result = $"if {this.Condition.StringValue} {this.Consequence.StringValue} ";
                if (this.Alternative != null)
                {
                    result += $"else {this.Alternative.StringValue}";
                }
                return result;
            }
        }
    }
}
