using System;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Storage;

namespace Essential.Templating.Razor.Host.Execution
{
    public class CurrentAppDomainExecutionHost : RazorExecutionHost
    {
        private readonly Lazy<TemplateFactory> _templateFactory =
            new Lazy<TemplateFactory>(() => new TemplateFactory());

        public CurrentAppDomainExecutionHost(ITextSourceProvider textSourceProvider) 
            : base(textSourceProvider, new RazorCompiler())
        {
            
        }

        public CurrentAppDomainExecutionHost(ITextSourceProvider textSourceProvider, RazorCompiler compiler) 
            : base(textSourceProvider, compiler)
        {
        }

        protected override TemplateFactory TemplateFactory
        {
            get { return _templateFactory.Value; }
        }
    }
}
