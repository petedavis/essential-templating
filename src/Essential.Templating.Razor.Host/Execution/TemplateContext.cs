﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Security;

namespace Essential.Templating.Razor.Host.Execution
{
    [Serializable]
    public class TemplateContext : ISerializable
    {
        public TemplateContext(TextWriter writer)
        {
            Contract.Requires(writer != null);
            Writer = writer;
        }

        protected TemplateContext(SerializationInfo info, StreamingContext context)
        {
            Writer = (TextWriter) info.GetValue("Writer", typeof (TextWriter));
        }

        public TextWriter Writer { get; protected internal set; }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Writer", Writer);
        }
    }
}
