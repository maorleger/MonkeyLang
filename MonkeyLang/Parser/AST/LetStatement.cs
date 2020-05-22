namespace MonkeyLang
{
    public class LetStatement : IStatement
    {
        public LetStatement(Token token, Identifier name, IExpression value)
        {
            this.Token = token;
            this.Name = name;
            this.Value = value;
        }

        public Token Token { get; }
        public Identifier Name { get; }
        public IExpression Value { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => $"{this.TokenLiteral} {this.Name.StringValue} = {this.Value?.StringValue};";
    }
}
