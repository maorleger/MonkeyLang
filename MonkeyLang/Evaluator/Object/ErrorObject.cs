using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class ErrorObject : IObject
    {
        public ErrorObject(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public ObjectType Type => ObjectType.Error;

        public string Inspect() => $"ERROR: {Message}";
    }
}
