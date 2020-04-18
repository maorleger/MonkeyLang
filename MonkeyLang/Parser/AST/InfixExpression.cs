using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class InfixExpression : IExpression
    {
        public InfixExpression(Token token, IExpression left, string op, IExpression right)
        {
            this.Token = token;
            this.Left = left;
            this.Operator = op;
            this.Right = right;
        }

        public Token Token { get; }
        public IExpression Left { get; }
        public string Operator { get; }
        public IExpression Right { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => $"({this.Left.StringValue} {this.Operator} {this.Right.StringValue})";
    }
}
