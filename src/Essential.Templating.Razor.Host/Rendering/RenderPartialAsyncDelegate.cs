using System.IO;
using System.Threading.Tasks;

namespace Essential.Templating.Razor.Host.Rendering
{
    public delegate Task RenderPartialAsyncDelegate(string id, TextWriter writer);
}
