using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Storage;
using Essential.Templating.Razor.Host.Templating;

namespace Essential.Templating.Razor.Host.Execution
{
    [Serializable]
    public class TemplateFactory
    {
        private readonly ConcurrentDictionary<string, TemplateReference> _references = 
            new ConcurrentDictionary<string, TemplateReference>(); 

        public void Attach(TemplateReference reference)
        {
            Contract.Requires<ArgumentNullException>(reference != null);

            var type = Type.GetType(reference.AssemblyQualifiedTypeName, false);
            if (type == null)
            {
                var message = string.Format("Type '{0}' is not loaded into current app domain.",
                    reference.AssemblyQualifiedTypeName);
                throw new ArgumentException(message, "reference");
            }
            if (!typeof (ITemplate).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("Template type is invalid. ITemplate interface implementation is required.");
            }
            _references[reference.Id] = reference;
        }

        public void Load(CompilationResult compilationResult)
        {
            Contract.Requires<ArgumentNullException>(compilationResult != null);

            Assembly.Load(compilationResult.Assembly);
            foreach (var templateReference in compilationResult.TemplateReferences)
            {
                _references[templateReference.Id] = templateReference;
            }
        }

        public bool IsAttached(string id)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(id));

            return _references.ContainsKey(id);
        }

        public ITemplate Create(string id)
        {
            var reference = _references[id];
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("RazorGeneratedTemplates"))
                .SelectMany(x => x.GetExportedTypes())
                .FirstOrDefault(x => x.AssemblyQualifiedName == reference.AssemblyQualifiedTypeName);
            //var type = Type.GetType(reference.AssemblyQualifiedTypeName);
            if (type == null)
            {
                var message = string.Format("Couldn't load template type '{0}'.", reference.AssemblyQualifiedTypeName);
                throw new InvalidOperationException(message);
            }
            var template = Activator.CreateInstance(type) as ITemplate;
            template.Id = id;
            template.FilePath = reference.FilePath;
            return template;
        }
    }
}
