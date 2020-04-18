namespace MonkeyLang
{
    public class LetStatement : IStatement
    {
        public LetStatement(Token token, Identifier name, IExpression value)
        {
            Token = token;
            Name = name;
            Value = value;
        }

        public Token Token { get; }
        public Identifier Name { get; }
        public IExpression Value { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => $"{TokenLiteral} {Name.StringValue} = {Value?.StringValue};";
    }
}
