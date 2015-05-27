using System.IO;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Execution;
using Essential.Templating.Razor.Host.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Templating.Razor.Host.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var provider = new InMemoryTextSourceProvider();
            provider.Put("test", "@inherits Essential.Templating.Razor.Host.Templating.Template<string>\r\nHello, @Model!");
            var host = new CurrentAppDomainExecutionHost(provider);
            var writer = new StringWriter();
            var context = new TemplateContext<string>("John", writer);
            host.ExecuteAsync("test", context).Wait();
            var builder = writer.GetStringBuilder();
            var renderedText = builder.ToString();
            writer.Dispose();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var provider = new InMemoryTextSourceProvider();
            provider.Put("test", "@inherits Essential.Templating.Razor.Host.Templating.Template<string>\r\nHello, @Model!");
            var host = SeparateAppDomainExecutionHost.Create(provider);
            var writer = new StringWriter();
            var context = new TemplateContext<string>("John", writer);
            host.ExecuteAsync("test", context).Wait();
            var builder = writer.GetStringBuilder();
            var renderedText = builder.ToString();
            writer.Dispose();
        }
    }
}
