using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Essential.Templating.Razor.Host.Execution;
using Essential.Templating.Razor.Host.Rendering;

namespace Essential.Templating.Razor.Host.Templating
{
    public interface ITemplate
    {
        string Id { get; set; }

        string FilePath { get; set; }

        TemplateContext Context { get; set; }

        string LayoutId { get; set; }

        bool IsPartial { get; set; }

        Action<TextWriter> RenderBodyDelegate { get; set; }

        Func<string, object, TextWriter, Task> RenderPartialAsyncDelegate { get; set; }

        IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        IDictionary<string, RenderAsyncDelegate> SectionWriters { get; }

        bool IsLayoutBeingRendered { get; set; }
        
        void EnsureRenderedBodyOrSections();
        Task ExecuteAsync();
    }
}