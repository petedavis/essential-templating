using System.IO;
using System.Threading.Tasks;

namespace Essential.Templating.Razor.Host.Rendering
{
    public delegate Task RenderAsyncDelegate(TextWriter writer);
}