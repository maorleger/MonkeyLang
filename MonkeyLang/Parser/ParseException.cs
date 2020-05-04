using System;

namespace MonkeyLang
{
    public class ParseException : Exception
    {
        public ParseException(string? message) : base(message)
        {
        }

        public ParseException()
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
