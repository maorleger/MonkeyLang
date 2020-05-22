namespace MonkeyLang
{
    public class ExpressionStatement : IStatement
    {
        public ExpressionStatement(Token token, IExpression expression)
        {
            this.Token = token;
            this.Expression = expression;
        }

        public Token Token { get; }
        public IExpression Expression { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => this.Expression != null ? this.Expression.StringValue : string.Empty;
    }
}
