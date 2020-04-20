using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class NullObject : IObject
    {
        public NullObject()
        {
        }

        public ObjectType Type => ObjectType.Null;

        public string Inspect => "null";
    }
}
