using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class ReturnValue : IObject
    {
        public ReturnValue(IObject value)
        {
            Value = value;
        }

        public IObject Value { get; }

        public ObjectType Type => ObjectType.Return;

        public string Inspect() => Value.Inspect();
    }
}
