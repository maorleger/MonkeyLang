using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class BooleanObject : IObject
    {
        public BooleanObject(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public ObjectType Type => ObjectType.Boolean;

        public string Inspect() => Value.ToString();
    }
}
