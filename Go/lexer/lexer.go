package lexer

import "github.com/maorleger/monkeylang/token"

type Lexer struct {
	input        string
	position     int
	readPosition int
	ch           byte
}

func (l *Lexer) readChar() {
	if l.readPosition >= len(l.input) {
		l.ch = 0
	} else {
		l.ch = l.input[l.readPosition]
	}
	l.position = l.readPosition
	l.readPosition++
}

var mapping = map[byte]token.TokenType{
	'=': token.ASSIGN,
	';': token.SEMICOLON,
	'(': token.LPAREN,
	')': token.RPAREN,
	'{': token.LBRACE,
	'}': token.RBRACE,
	',': token.COMMA,
	'+': token.PLUS,
}

func (l *Lexer) NextToken() token.Token {
	var tok token.Token

	l.skipWhitespace()

	if tokType, ok := mapping[l.ch]; ok {
		tok = token.NewToken(tokType, l.ch)
		l.readChar()
	} else if isLetter(l.ch) {
		tok.Literal = l.readIdentifier()
		tok.Type = token.LookupIdent(tok.Literal)
	} else if isDigit(l.ch) {
		tok.Type = token.INT
		tok.Literal = l.readNumber()
	} else if l.ch == 0 {
		tok.Literal = ""
		tok.Type = token.EOF
	} else {
		tok = token.NewToken(token.ILLEGAL, l.ch)
		tok.Type = token.ILLEGAL
		tok.Literal = string(l.ch)
	}

	return tok
}

func (l *Lexer) skipWhitespace() {
	for l.ch == ' ' || l.ch == '\t' || l.ch == '\n' || l.ch == '\r' {
		l.readChar()
	}
}

func isLetter(ch byte) bool {
	return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_'
}

func (l *Lexer) readIdentifier() string {
	position := l.position
	for isLetter(l.ch) {
		l.readChar()
	}

	return l.input[position:l.position]
}

func isDigit(ch byte) bool {
	return '0' <= ch && ch <= '9'
}

func (l *Lexer) readNumber() string {
	pos := l.position
	for isDigit(l.ch) {
		l.readChar()
	}

	return l.input[pos:l.position]
}

func NewLexer(input string) *Lexer {
	l := &Lexer{input: input}
	l.readChar()
	return l
}
