using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MonkeyLang
{
    public class Program : INode
    {
        public Program(IEnumerable<IStatement> statements)
        {
            Statements = statements.ToImmutableList();
        }

        public IImmutableList<IStatement> Statements { get; }

        public string TokenLiteral =>
            Statements
            .DefaultIfEmpty(new NullStatement())
            .First()
            .TokenLiteral;

        public string StringValue =>
            Statements
            .Aggregate(string.Empty, (acc, st) => acc + st.StringValue);

        private class NullStatement : IStatement
        {
            public string TokenLiteral => string.Empty;
            public string StringValue => "NullStatement";
        }
    }
}
