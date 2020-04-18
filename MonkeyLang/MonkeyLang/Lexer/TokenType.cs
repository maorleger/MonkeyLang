namespace MonkeyLang
{
    public enum TokenType
    {
        Illegal,
        EOF,

        Ident,
        Int,

        Assign,
        Plus,
        Minus,
        Bang,
        Asterisk,
        Slash,

        LT,
        GT,
        Eq,
        Not_Eq,

        Comma,
        Semicolon,

        LParen,
        RParen,
        LBrace,
        RBrace,

        Function,
        Let,
        True,
        False,
        If,
        Else,
        Return
    }
}
