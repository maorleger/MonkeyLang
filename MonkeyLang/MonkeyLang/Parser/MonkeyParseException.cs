using System;

namespace MonkeyLang
{
    public class MonkeyParseException : Exception
    {
        public MonkeyParseException(string? message) : base(message)
        {
        }

        public MonkeyParseException()
        {
        }

        public MonkeyParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
