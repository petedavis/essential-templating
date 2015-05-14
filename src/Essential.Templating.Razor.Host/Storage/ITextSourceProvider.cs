using System.Globalization;

namespace Essential.Templating.Razor.Host.Storage
{
    public interface ITextSourceProvider
    {
        TextSource Load(string id, CultureInfo culture = null);
    }
}
