using System;

namespace MonkeyLang
{
    public interface INode
    {
        public String TokenLiteral { get; }
        public String StringValue { get; }
    }
}
