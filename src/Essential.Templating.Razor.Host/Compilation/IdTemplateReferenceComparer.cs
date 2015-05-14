using System.Collections.Generic;
using Essential.Templating.Razor.Host.Storage;

namespace Essential.Templating.Razor.Host.Compilation
{
    internal class IdTemplateReferenceComparer : IEqualityComparer<TemplateReference>
    {
        public bool Equals(TemplateReference x, TemplateReference y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(TemplateReference obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
