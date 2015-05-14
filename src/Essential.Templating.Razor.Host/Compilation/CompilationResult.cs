using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Essential.Templating.Razor.Host.Storage;

namespace Essential.Templating.Razor.Host.Compilation
{
    [Serializable]
    public class CompilationResult
    {
        private readonly ReadOnlyCollection<TemplateReference> _templateReferences;

        private readonly byte[] _assembly;

        public CompilationResult(IList<TemplateReference> templateReferences, byte[] assembly)
        {
            Contract.Requires(templateReferences != null);
            Contract.Requires(assembly != null);

            _templateReferences = new ReadOnlyCollection<TemplateReference>(templateReferences);
            _assembly = assembly;
        }

        public byte[] Assembly
        {
            get { return _assembly; }
        }

        public IReadOnlyCollection<TemplateReference> TemplateReferences
        {
            get { return _templateReferences; }
        }
    }
}