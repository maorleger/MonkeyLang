using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class BooleanObject : IObject
    {
        public static BooleanObject True = new BooleanObject(true);
        public static BooleanObject False = new BooleanObject(false);

        private BooleanObject(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public ObjectType Type => ObjectType.Boolean;

        public string Inspect() => Value.ToString();

        internal static BooleanObject FromNative(bool value) => value ? True : False;
    }
}
