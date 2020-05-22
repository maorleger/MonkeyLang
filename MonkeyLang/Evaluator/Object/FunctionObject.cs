using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class FunctionObject : IObject
    {
        public FunctionObject(IEnumerable<Identifier> parameters, BlockStatement body, RuntimeEnvironment environment)
        {
            this.Parameters = parameters.ToImmutableList();
            this.Body = body;
            this.Environment = environment;
        }

        public IImmutableList<Identifier> Parameters { get; }
        public BlockStatement Body { get; }
        public RuntimeEnvironment Environment { get; }

        public ObjectType Type => ObjectType.Function;

        public string Inspect()
        {
            var sb = new StringBuilder("fn(");
            sb.AppendJoin(", ", this.Parameters.Select(p => p.StringValue));
            sb.Append(")");
            sb.AppendLine();
            sb.Append(this.Body.StringValue);
            sb.AppendLine();
            sb.Append("}");
            return sb.ToString();
        }
    }
}
