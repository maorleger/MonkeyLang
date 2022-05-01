namespace MonkeyLang
{
    public class IndexExpression : IExpression
    {
        public IndexExpression(Token token, IExpression left, IExpression index)
        {
            this.Token = token;
            this.Left = left;
            this.Index = index;
        }

        public Token Token { get; }
        public IExpression Left { get; }
        public IExpression Index { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => $"({this.Left.StringValue}[{this.Index.StringValue}])";
    }
}
