using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Essential.Templating.Razor.Host.Rendering;
using Essential.Templating.Razor.Host.Templating;

namespace Essential.Templating.Razor.Host.Execution
{
    public abstract class ExecutionHostBase : IRazorExecutionHost
    {
        private readonly TemplateFactory _factory;

        public Task ExecuteAsync(string id, TemplateContext context)
        {
        }

        private async Task RenderAsync(ITemplate template, TemplateContext context)
        {
            template.RenderPartialAsyncDelegate = (s, o, arg3) =>
            {
               
                var ctx = new TemplateContext() {Writer = context.Writer};
                return ExecuteAsync(s, ctx);
            };
            var bodyWriter = await RenderPageAsync(template, context);
            await RenderLayoutAsync(template, context, bodyWriter);
        }

        private async Task<IBufferedTextWriter> RenderPageAsync(ITemplate template, TemplateContext context)
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
                await RenderPageCoreAsync(template, context, false);
                return bufferedWriter;
            }
            finally
            {
                context.Writer = oldWriter;
                //context.ExecutingFilePath = oldFilePath;
                writer.Dispose();
            }
        }

        private async Task RenderPageCoreAsync(ITemplate page, TemplateContext context, bool isPartial)
        {
            page.IsPartial = isPartial;
            page.Context = context;

            _pageActivator.Activate(page, context);
            await page.ExecuteAsync();
        }

        private async Task RenderLayoutAsync(
            ITemplate template,
            TemplateContext context,
            IBufferedTextWriter bodyWriter)
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

                var layoutPage = GetLayoutPage(context, previousPage.LayoutId);

                // Notify the previous page that any writes that are performed on it are part of sections being written
                // in the layout.
                previousPage.IsLayoutBeingRendered = true;
                layoutPage.PreviousSectionWriters = previousPage.SectionWriters;
                layoutPage.RenderBodyDelegate = bodyWriter.CopyTo;
                bodyWriter = await RenderPageAsync(layoutPage, context);

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

        private ITemplate GetLayoutPage(TemplateContext context, string layoutPath)
        {
            var layoutPageResult = _viewEngine.FindPage(context, layoutPath);
            if (layoutPageResult.Page == null)
            {
                var locations = Environment.NewLine +
                                string.Join(Environment.NewLine, layoutPageResult.SearchedLocations);
                throw new InvalidOperationException(Resources.FormatLayoutCannotBeLocated(layoutPath, locations));
            }

            var layoutPage = layoutPageResult.Page;
            return layoutPage;
        }


    }
}
