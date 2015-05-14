using Essential.Templating.Razor.Host.Execution;

namespace Essential.Templating.Razor.Host.Templating
{
    public abstract class Template<T> : Template
    {
        public T Model { get; private set; }

        protected override void SetContext(TemplateContext context)
        {
            if (context == null)
            {
                base.SetContext(null);
            }
            var typedContext = context as TemplateContext<T>;
            if (typedContext != null)
            {
                Model = typedContext.Model;
            }
            base.SetContext(context);
        }
    }
}
