using System;
using System.Runtime.Serialization;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;

namespace Essential.Templating.Razor.Host.Compilation
{
    [Serializable]
    public class RazorParserException : RazorException
    {
        protected RazorParserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Errors = (RazorError[]) info.GetValue("Errors", typeof (RazorError[]));
        }

        internal RazorParserException(RazorError[] errors) : base("Parsing stage failed. See Errors property for more details.")
        {
            Errors = errors;
        }

        public RazorError[] Errors { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Errors", Errors);
            base.GetObjectData(info, context);
        }
    }
}
