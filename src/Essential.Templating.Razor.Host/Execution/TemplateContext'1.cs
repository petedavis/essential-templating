using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security;

namespace Essential.Templating.Razor.Host.Execution
{
    [Serializable]
    public class TemplateContext<T> : TemplateContext
    {
        public TemplateContext(T model, TextWriter writer) : base(writer)
        {
            Model = model;
        }

        public T Model { get; private set; }

        protected TemplateContext(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Model =  (T) info.GetValue("Model", typeof (T));
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Model", Model);
            base.GetObjectData(info, context);
        }
    }
}
