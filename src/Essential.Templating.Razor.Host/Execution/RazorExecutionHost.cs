using System;
using System.Collections.Concurrent;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Storage;
using Essential.Templating.Razor.Host.Templating;

namespace Essential.Templating.Razor.Host.Execution
{
    public class RazorExecutionHost : IDisposable
    {
        private ConcurrentDictionary<TemplateReference, int> _templateReferences;

        private readonly RazorExecutionSandbox _sandbox;

        private readonly AppDomain _sandboxDomain;

        public RazorExecutionHost()
        {
            _templateReferences = new ConcurrentDictionary<TemplateReference, int>();
        }

        public void Attach(CompilationResult result)
        {
        }

        public void Attach(TemplateReference template)
        {
            
        }

        public void Execute<TTemplate>(string id, TemplateContext context)
        {
        }
 
        public void Dispose()
        {
        }
    }
}
