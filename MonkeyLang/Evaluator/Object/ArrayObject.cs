using System;
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
            Elements = elements.ToImmutableList();
        }

        public IImmutableList<IObject> Elements { get; }

        public ObjectType Type => ObjectType.Array;

        public string Inspect()
        {
            StringBuilder sb = new StringBuilder("[");
            sb.AppendJoin(", ", Elements.Select(el => el.Inspect()));
            sb.AppendLine("]");

            return sb.ToString();
        }
    }
}
