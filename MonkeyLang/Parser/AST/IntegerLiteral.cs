namespace MonkeyLang
{
    public class IntegerLiteral : IExpression
    {
        public IntegerLiteral(Token token, int value)
        {
            this.Token = token;
            this.Value = value;
        }

        public Token Token { get; }
        public int Value { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => this.Token.Literal;
    }
}
