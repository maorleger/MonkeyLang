using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace MonkeyLang
{
    public class ErrorObject : IObject
    {
        public ErrorObject(IEnumerable<string> errorMessages)
        {
            Messages = errorMessages.ToImmutableList();
        }

        public IImmutableList<string> Messages { get; }

        public ObjectType Type => ObjectType.Error;

        public string Inspect() => $"ERRORS:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, Messages)}";
    }
}
