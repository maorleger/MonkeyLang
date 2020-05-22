using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class ArrayObject : IObject
    {
        public ArrayObject(IEnumerable<IObject> elements)
        {
            this.Elements = elements.ToImmutableList();
        }

        public IImmutableList<IObject> Elements { get; }

        public ObjectType Type => ObjectType.Array;

        public string Inspect()
        {
            var sb = new StringBuilder("[");
            sb.AppendJoin(", ", this.Elements.Select(el => el.Inspect()));
            sb.AppendLine("]");

            return sb.ToString();
        }
    }
}
