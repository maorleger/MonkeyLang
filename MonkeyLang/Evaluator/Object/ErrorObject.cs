using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public class ErrorObject : IObject
    {
        public ErrorObject(IEnumerable<string> errorMessages)
        {
            Messages = errorMessages;
        }

        public IEnumerable<string> Messages { get; }

        public ObjectType Type => ObjectType.Error;

        public string Inspect() => $"ERRORS:{Environment.NewLine}{string.Join(Environment.NewLine, Messages)}";
    }
}
