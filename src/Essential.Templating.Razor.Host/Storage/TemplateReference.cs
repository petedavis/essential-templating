using System;
using System.Diagnostics.Contracts;

namespace Essential.Templating.Razor.Host.Storage
{
    public class TemplateReference
    {
        private readonly string _id;

        private readonly string _assemblyQualifiedTypeName;

        private readonly string _filePath;

        internal TemplateReference(string id, string assemblyQualifiedTypeName, string filePath = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(assemblyQualifiedTypeName));

            _id = id;
            _assemblyQualifiedTypeName = assemblyQualifiedTypeName;
            _filePath = filePath;
        }

        public string Id
        {
            get { return _id; }
        }

        public string AssemblyQualifiedTypeName
        {
            get { return _assemblyQualifiedTypeName; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }
    }
}
