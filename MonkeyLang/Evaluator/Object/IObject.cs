using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public interface IObject
    {
        ObjectType Type { get; }
        string Inspect();
    }
}
