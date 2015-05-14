using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Storage;
using Essential.Templating.Razor.Host.Templating;

namespace Essential.Templating.Razor.Host.Execution
{
    public static class RazorExecutionHostExtensions
    {
        public static void Execute(this IRazorExecutionHost host, CompilationResult result, string id, TemplateContext context)
        {
            
        }

        public static void ExecuteFirst(this IRazorExecutionHost host, CompilationResult result, TemplateContext context)
        {
            
        }

        public static void Execute(this IRazorExecutionHost host, TemplateReference reference, TemplateContext context)
        {
            host.Attach(reference);
            host.Execute(reference.Id, context);
        }
    }
}
