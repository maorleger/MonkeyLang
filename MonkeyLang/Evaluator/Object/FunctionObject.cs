using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class FunctionObject : IObject
    {
        public FunctionObject(IEnumerable<Identifier> parameters, BlockStatement body, IEnvironment environment)
        {
            Parameters = parameters.ToImmutableList();
            Body = body;
            Environment = new MonkeyEnvironment(environment);
        }

        public IImmutableList<Identifier> Parameters { get; }
        public BlockStatement Body { get; }
        public IEnvironment Environment { get; }

        public ObjectType Type => ObjectType.Function;

        public string Inspect()
        {
            StringBuilder sb = new StringBuilder("fn(");
            sb.AppendJoin(", ", Parameters.Select(p => p.StringValue));
            sb.Append(")");
            sb.AppendLine();
            sb.Append(Body.StringValue);
            sb.AppendLine();
            sb.Append("}");
            return sb.ToString();
        }
    }
}
