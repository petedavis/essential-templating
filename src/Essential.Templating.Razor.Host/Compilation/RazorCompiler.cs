using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Essential.Templating.Razor.Host.Storage;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Text;

namespace Essential.Templating.Razor.Host.Compilation
{
    public class RazorCompiler
    {
        private readonly RazorTemplateEngine _engine;

        public RazorCompiler()
        {
            var host = new RazorEngineHost(new CSharpRazorCodeLanguage());
            _engine = new RazorTemplateEngine(host);
        }

        public CompilationResult Compile(TextSource source)
        {
            return CompileMany(new []{source});
        }

        public CompilationResult CompileMany(TextSource[] sources)
        {
            Contract.Requires<ArgumentNullException>(sources != null, "sources");
            if (sources.GroupBy(x => x.Id).Count() != sources.Count())
            {
                throw new ArgumentException("All sources should have an unique id.", "sources");
            }
            var results = sources
                .AsParallel()
                .Select(x => new {source = x, results = _engine.GenerateCode(new BufferingTextReader(x.Reader))})
                .ToList();
            if (results.Any(x => !x.results.Success))
            {
                var errors = results.SelectMany(x => x.results.ParserErrors).ToArray();
            }
        }
    }
}
