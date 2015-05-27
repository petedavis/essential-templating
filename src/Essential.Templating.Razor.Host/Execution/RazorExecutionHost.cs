using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Rendering;
using Essential.Templating.Razor.Host.Storage;
using Essential.Templating.Razor.Host.Templating;

namespace Essential.Templating.Razor.Host.Execution
{
    [Serializable]
    public abstract class RazorExecutionHost : MarshalByRefObject, IRazorExecutionHost
    {
        protected ITextSourceProvider _textSourceProvider;

        protected RazorCompiler _compiler;

        protected RazorExecutionHost()
        {
        }

        protected RazorExecutionHost(ITextSourceProvider textSourceProvider, RazorCompiler compiler)
        {
            Contract.Requires(textSourceProvider != null);
            Contract.Requires(compiler != null);

            _textSourceProvider = textSourceProvider;
            _compiler = compiler;
        }

        public bool Attach(TemplateReference reference)
        {
            var replaced = TemplateFactory.IsAttached(reference.Id);
            TemplateFactory.Attach(reference);
            return replaced;
        }

        public bool CanExecute(string id)
        {
            return TemplateFactory.IsAttached(id) || _textSourceProvider.CanLoad(id);
        }

        [SecurityCritical]
        public virtual Task ExecuteAsync(string id, TemplateContext context, bool forceRecompile = false)
        {
            var template = Load(id, forceRecompile);
            if (template == null)
            {
                throw new InvalidOperationException("Not Found");
            }
            return RenderAsync(template, context, forceRecompile);
        }

        protected abstract TemplateFactory TemplateFactory { get; }

        [SecurityCritical]
        protected ITemplate Load(string id, bool forceRecompile)
        {
            ITemplate template = null;
            if (TemplateFactory.IsAttached(id) && !forceRecompile)
            {
                template = TemplateFactory.Create(id);
            }
            else
            {
                var source = _textSourceProvider.Load(id);
                if (source == null && !TemplateFactory.IsAttached(id))
                {
                    return null;
                }
                if (TemplateFactory.IsAttached(id))
                {
                    template = TemplateFactory.Create(id);
                }
                else
                {
                    var compilationResults = _compiler.Compile(source);
                    TemplateFactory.Load(compilationResults);
                    template = TemplateFactory.Create(id);
                }
            }
            return template;
        }

        protected async Task RenderAsync(ITemplate template, TemplateContext context, bool forceRecompile)
        {
            template.RenderPartialAsyncDelegate = (s, o, writer) =>
            {
                var ctx = new TemplateContext(writer);
                return ExecuteAsync(s, ctx, forceRecompile);
            };
            var bodyWriter = await RenderTemplateAsync(template, context);
            await RenderLayoutAsync(template, context, bodyWriter, forceRecompile);
        }

        private async Task<IBufferedTextWriter> RenderTemplateAsync(ITemplate template, TemplateContext context)
        {
            var razorTextWriter = new RazorTextWriter(context.Writer, context.Writer.Encoding);
            var writer = (TextWriter)razorTextWriter;
            var bufferedWriter = (IBufferedTextWriter)razorTextWriter;

            // The writer for the body is passed through the ViewContext, allowing things like HtmlHelpers
            // and ViewComponents to reference it.
            var oldWriter = context.Writer;
            //var oldFilePath = context.ExecutingFilePath;
            context.Writer = writer;
            //context.ExecutingFilePath = context.Path;

            try
            {
                await RenderTemplateCoreAsync(template, context, false);
                return bufferedWriter;
            }
            finally
            {
                context.Writer = oldWriter;
                //context.ExecutingFilePath = oldFilePath;
                writer.Dispose();
            }
        }

        private static async Task RenderTemplateCoreAsync(ITemplate template, TemplateContext context, bool isPartial)
        {
            template.IsPartial = isPartial;
            template.Context = context;
            //_templateInitializer.Initialize(template, context);
            await template.ExecuteAsync();
        }

        private async Task RenderLayoutAsync(
            ITemplate template,
            TemplateContext context,
            IBufferedTextWriter bodyWriter,
            bool forceRecompile)
        {
            // A layout page can specify another layout page. We'll need to continue
            // looking for layout pages until they're no longer specified.
            var previousPage = template;
            var renderedLayouts = new List<ITemplate>();
            while (!string.IsNullOrEmpty(previousPage.LayoutId))
            {
                if (!bodyWriter.IsBuffering)
                {
                    // Once a call to RazorPage.FlushAsync is made, we can no longer render Layout pages - content has
                    // already been written to the client and the layout content would be appended rather than surround
                    // the body content. Throwing this exception wouldn't return a 500 (since content has already been
                    // written), but a diagnostic component should be able to capture it.

                    var message = "TODO";
                    //var message = Resources.FormatLayoutCannotBeRendered(Path, nameof(Razor.RazorPage.FlushAsync));
                    throw new InvalidOperationException(message);
                }

                var layoutPage = GetLayoutPage(context, previousPage.LayoutId, forceRecompile);

                // Notify the previous page that any writes that are performed on it are part of sections being written
                // in the layout.
                previousPage.IsLayoutBeingRendered = true;
                layoutPage.PreviousSectionWriters = previousPage.SectionWriters;
                layoutPage.RenderBodyDelegate = bodyWriter.CopyTo;
                bodyWriter = await RenderTemplateAsync(layoutPage, context);

                renderedLayouts.Add(layoutPage);
                previousPage = layoutPage;
            }

            // Ensure all defined sections were rendered or RenderBody was invoked for page without defined sections.
            foreach (var layoutPage in renderedLayouts)
            {
                layoutPage.EnsureRenderedBodyOrSections();
            }

            if (bodyWriter.IsBuffering)
            {
                // Only copy buffered content to the Output if we're currently buffering.
                await bodyWriter.CopyToAsync(context.Writer);
            }
        }

        private ITemplate GetLayoutPage(TemplateContext context, string id, bool forceRecompile)
        {
            var template = Load(id, forceRecompile);
            if (template == null)
            {
                throw new InvalidOperationException("Layout was not found");
            }
            template.Context = context;
            return template;
        }
    }
}
