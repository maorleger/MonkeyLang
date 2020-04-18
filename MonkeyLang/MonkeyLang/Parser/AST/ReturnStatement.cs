namespace MonkeyLang
{
    public class ReturnStatement : IStatement
    {
        public ReturnStatement(Token token, IExpression returnValue)
        {
            this.Token = token;
            this.ReturnValue = returnValue;
        }

        public Token Token { get; }
        public IExpression ReturnValue { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => $"{TokenLiteral} {ReturnValue?.StringValue};";
    }
}
