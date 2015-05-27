using System;
using System.Reflection;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Storage;

namespace Essential.Templating.Razor.Host.Execution
{
    internal class RazorExecutionSandbox : MarshalByRefObject
    {
        public static RazorExecutionSandbox Create(AppDomain domain, ITextSourceProvider provider)
        {
            var asm = Assembly.GetExecutingAssembly();
            var sandbox = (RazorExecutionSandbox)Activator.CreateInstance(domain,
                asm.FullName, 
                typeof(RazorExecutionSandbox).FullName).Unwrap();

            return sandbox;
        }
    }
}
