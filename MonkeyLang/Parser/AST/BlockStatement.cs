using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MonkeyLang
{
    public class BlockStatement : IStatement
    {
        public BlockStatement(Token token, IEnumerable<IStatement> statements)
        {
            Token = token;
            Statements = statements.ToImmutableList();
        }

        public Token Token { get; }
        public IImmutableList<IStatement> Statements { get; }

        public string TokenLiteral => Token.Literal;

        public string StringValue => Statements.Aggregate(string.Empty, (acc, st) => acc + st.StringValue);
    }
}