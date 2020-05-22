namespace MonkeyLang
{
    public class StringLiteral : IExpression
    {
        public StringLiteral(Token token, string value)
        {
            this.Token = token;
            this.Value = value;
        }

        public Token Token { get; }
        public string Value { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => this.Value;
    }
}
