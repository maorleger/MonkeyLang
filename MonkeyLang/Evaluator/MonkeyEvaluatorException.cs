using System;
using System.Runtime.Serialization;

namespace MonkeyLang
{
    [Serializable]
    internal class MonkeyEvaluatorException : Exception
    {
        public MonkeyEvaluatorException()
        {
        }

        public MonkeyEvaluatorException(string? message) : base(message)
        {
        }

        public MonkeyEvaluatorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MonkeyEvaluatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}