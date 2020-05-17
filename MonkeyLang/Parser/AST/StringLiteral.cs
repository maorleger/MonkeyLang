namespace MonkeyLang
{
    public class StringLiteral : IExpression
    {
        public StringLiteral(Token token, string value)
        {
            Token = token;
            Value = value;
        }

        public Token Token { get; }
        public string Value { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => Value;
    }
}
