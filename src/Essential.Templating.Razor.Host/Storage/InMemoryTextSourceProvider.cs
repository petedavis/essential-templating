using System.Collections.Concurrent;
using System.IO;

namespace Essential.Templating.Razor.Host.Storage
{
    public class InMemoryTextSourceProvider : ITextSourceProvider
    {
        private readonly ConcurrentDictionary<string, string> _templates = 
            new ConcurrentDictionary<string, string>(); 

        public bool CanLoad(string id)
        {
            return _templates.ContainsKey(id);
        }

        public TextSource Load(string id)
        {
            string template = null;
            var loaded = _templates.TryGetValue(id, out template);
            return loaded ? new TextSource(new StringReader(template), id) : null;
        }

        public void Put(string id, string template)
        {
            _templates[id] = template;
        }
    }
}
