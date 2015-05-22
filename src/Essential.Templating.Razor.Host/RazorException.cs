using System;
using System.Runtime.Serialization;

namespace Essential.Templating.Razor.Host
{
    [Serializable]
    public class RazorException : Exception
    {
        public RazorException(string message) : base(message)
        {
        }

        public RazorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RazorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
