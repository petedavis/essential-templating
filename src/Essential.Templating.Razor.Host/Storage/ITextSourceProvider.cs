namespace Essential.Templating.Razor.Host.Storage
{
    public interface ITextSourceProvider
    {
        bool CanLoad(string id);

        TextSource Load(string id);
    }
}
