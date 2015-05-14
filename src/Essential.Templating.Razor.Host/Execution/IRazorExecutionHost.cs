using System.Threading.Tasks;

namespace Essential.Templating.Razor.Host.Execution
{
    public interface IRazorExecutionHost
    {
        Task ExecuteAsync(string id, TemplateContext context);
    }
}
