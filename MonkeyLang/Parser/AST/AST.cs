using System.Collections.Generic;
using System.Collections.Immutable;

namespace MonkeyLang
{
    public class AST
    {
        public AST(Program program, IEnumerable<ParseException> errors)
        {
            this.Program = program;
            this.Errors = errors.ToImmutableList();
        }

        public Program Program { get; }
        public IImmutableList<ParseException> Errors { get; }
        public bool HasErrors => this.Errors.Count > 0;
    }
}
