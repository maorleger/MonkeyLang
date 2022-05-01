namespace MonkeyLang
{
    public class Boolean : IExpression
    {
        public Boolean(Token token, bool value)
        {
            this.Token = token;
            this.Value = value;
        }

        public Token Token { get; }
        public bool Value { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => this.TokenLiteral;
    }
}
