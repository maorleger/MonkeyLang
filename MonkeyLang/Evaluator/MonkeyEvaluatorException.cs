using System;
using System.Runtime.Serialization;

namespace MonkeyLang
{
    [Serializable]
    internal class EvaluatorException : Exception
    {
        public EvaluatorException()
        {
        }

        public EvaluatorException(string? message) : base(message)
        {
        }

        public EvaluatorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EvaluatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}