namespace MonkeyLang
{
    public struct Token
    {
        public Token(TokenType tokenType, string literal)
        {
            this.Type = tokenType;
            this.Literal = literal;
        }

        public TokenType Type { get; }
        public string Literal { get; }

        public override string ToString()
        {
            return $"Token [Type='{Type}', Literal='{Literal}']";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Token other)
            {
                return this.Type == other.Type && this.Literal == other.Literal;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Type, Literal);
        }

        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !(left == right);
        }
    }
}
