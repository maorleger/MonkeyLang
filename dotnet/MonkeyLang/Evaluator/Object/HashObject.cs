using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class HashObject : IObject
    {
        public HashObject(IReadOnlyDictionary<IObject, IObject> pairs)
        {
            this.Pairs = pairs.ToImmutableDictionary();
        }

        public IImmutableDictionary<IObject, IObject> Pairs { get; }

        public ObjectType Type => ObjectType.Hash;

        public string Inspect()
        {
            var sb = new StringBuilder("{");
            sb.AppendJoin(", ", this.Pairs.Select(kv => $"{kv.Key.Inspect()}:{kv.Value.Inspect()}"));
            sb.Append("}");
            return sb.ToString();
        }
    }
}
