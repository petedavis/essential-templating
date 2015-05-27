using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.CodeAnalysis;

namespace Essential.Templating.Razor.Host.Compilation
{
    [Serializable]
    public class RazorCompilerException : RazorException
    {
        public RazorCompilerException(Diagnostic[] diagnostics) : base(string.Format("Compilation stage failed: {0} errors found.", diagnostics.Length))
        {
            Errors = diagnostics.Select(x => x.ToString()).ToArray();
        }

        public string[] Errors { get; private set; }

        protected RazorCompilerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Errors = (string[]) info.GetValue("Errors", typeof (string[]));
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Errors", Errors);
            base.GetObjectData(info, context);
        }
    }
}
