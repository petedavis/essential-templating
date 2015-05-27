using System.Security;
using System.Threading.Tasks;
using Essential.Templating.Razor.Host.Storage;

namespace Essential.Templating.Razor.Host.Execution
{
    public interface IRazorExecutionHost
    {
        bool Attach(TemplateReference reference);

        bool CanExecute(string id);

        [SecurityCritical]
        Task ExecuteAsync(string id, TemplateContext context, bool forceRecompile = false);
    }
}
