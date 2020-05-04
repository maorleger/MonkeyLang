using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        public bool HasErrors => Errors.Count > 0;
    }
}
