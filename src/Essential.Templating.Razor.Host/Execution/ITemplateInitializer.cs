using Essential.Templating.Razor.Host.Templating;

namespace Essential.Templating.Razor.Host.Execution
{
    public interface ITemplateInitializer
    {
        void Initialize(ITemplate template, TemplateContext context);
    }
}
