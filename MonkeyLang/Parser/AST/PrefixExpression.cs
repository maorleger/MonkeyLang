namespace MonkeyLang
{
    public class PrefixExpression : IExpression
    {
        public PrefixExpression(Token token, TokenType op, IExpression right)
        {
            this.Token = token;
            this.Operator = op;
            this.Right = right;
        }

        public Token Token { get; }
        public TokenType Operator { get; }
        public IExpression Right { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => $"({this.Token.Literal}{this.Right.StringValue})";
    }
}
