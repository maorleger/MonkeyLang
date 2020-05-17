using System.ComponentModel;

namespace MonkeyLang
{
    public enum TokenType
    {
        Illegal,
        EOF,

        Ident,
        Int,

        [Description("=")]
        Assign,
        [Description("+")]
        Plus,
        [Description("-")]
        Minus,
        [Description("!")]
        Bang,
        [Description("*")]
        Asterisk,
        [Description("/")]
        Slash,

        [Description("<")]
        LT,
        [Description(">")]
        GT,
        [Description("==")]
        Eq,
        [Description("!=")]
        Not_Eq,

        [Description(",")]
        Comma,
        [Description(";")]
        Semicolon,
        [Description(":")]
        Colon,

        [Description("(")]
        LParen,
        [Description(")")]
        RParen,
        [Description("{")]
        LBrace,
        [Description("}")]
        RBrace,

        Function,
        Let,
        True,
        False,
        If,
        Else,
        Return,
        String,

        [Description("[")]
        LBracket,
        [Description("]")]
        RBracket
    }
}
