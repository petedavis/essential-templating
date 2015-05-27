using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security;
using System.Web.Razor;
using System.Web.Razor.Text;
using Essential.Templating.Razor.Host.Storage;
using Essential.Templating.Razor.Host.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;

namespace Essential.Templating.Razor.Host.Compilation
{
    [Serializable]
    public class RazorCompiler
    {
        private readonly RazorTemplateEngine _engine;

        [SecurityCritical]
        public RazorCompiler()
        {
            var host = new RazorEngineHost(new CSharpRazorCodeLanguage());
            host.NamespaceImports.Add("Essential.Templating.Razor.Host.Templating");
            host.DefaultBaseClass = "Essential.Templating.Razor.Host.Templating.Template";
            _engine = new RazorTemplateEngine(host);
        }

        [SecurityCritical]
        public CompilationResult Compile(TextSource source)
        {
            return CompileMany(new []{source});
        }

        [SecurityCritical]
        public CompilationResult CompileMany(TextSource[] sources)
        {
            Contract.Requires<ArgumentNullException>(sources != null, "sources");
            if (sources.GroupBy(x => x.Id).Count() != sources.Count())
            {
                throw new ArgumentException("All sources should have an unique id.", "sources");
            }
            var assemblyName = string.Format("RazorGeneratedTemplates_{0}", Guid.NewGuid().ToString("N"));
            var results = sources
                .AsParallel()
                .Select(x =>
                {
                    const string ns = "RazorGeneratedTemplates";
                    var className = string.Format("Template_{0}", Guid.NewGuid().ToString("N"));
                    var fullyQualifiedName = string.Format("{0}.{1}, {2}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", ns, className, assemblyName);
                    return new
                    {
                        source = x,
                        results = _engine.GenerateCode(new BufferingTextReader(x.Reader), className, ns, x.FileName),
                        fullyQualifiedName
                    };
                })
                .ToList();
            if (results.Any(x => !x.results.Success))
            {
                var errors = results.SelectMany(x => x.results.ParserErrors).ToArray();
                throw new RazorParserException(errors);
            }
            var trees = results.Select(x => x.results.GeneratedCode)
                .AsParallel()
                .Select(ToCode)
                .ToList();
            var compilation = CSharpCompilation.Create(assemblyName)
                .AddReferences(
                    MetadataReference.CreateFromAssembly(typeof (object).Assembly),
                    MetadataReference.CreateFromAssembly(typeof (ITemplate).Assembly)
                )
                .AddSyntaxTrees(trees)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    var diagnostics = result.Diagnostics;
                    var compilationErrors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
                    throw new RazorCompilerException(compilationErrors);
                }
                ms.Position = 0;
                return new CompilationResult(
                    results.Select(x => new TemplateReference(x.source.Id, x.fullyQualifiedName, x.source.FileName)).ToList(),
                    ms.ToArray());
            }
        }

        [SecurityCritical]
        private SyntaxTree ToCode(CodeCompileUnit codeCompileUnit)
        {
            var writer = new StringWriter();
            var provider = new CSharpCodeProvider();
            provider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, new CodeGeneratorOptions());
            var code =  writer.GetStringBuilder().ToString();
            return CSharpSyntaxTree.ParseText(code);
        }
    }
}
