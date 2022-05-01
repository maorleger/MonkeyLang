namespace MonkeyLang
{
    public class InfixExpression : IExpression
    {
        public InfixExpression(Token token, IExpression left, TokenType op, IExpression right)
        {
            this.Token = token;
            this.Left = left;
            this.Operator = op;
            this.Right = right;
        }

        public Token Token { get; }
        public IExpression Left { get; }
        public TokenType Operator { get; }
        public IExpression Right { get; }

        public string TokenLiteral => this.Token.Literal;

        public string StringValue => $"({this.Left.StringValue} {this.Token.Literal} {this.Right.StringValue})";
    }
}
