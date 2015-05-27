using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Essential.Templating.Razor.Host.Compilation;
using Essential.Templating.Razor.Host.Storage;

namespace Essential.Templating.Razor.Host.Execution
{
    public class SeparateAppDomainExecutionHost : RazorExecutionHost
    {
        private AppDomain _appDomain;

        private TemplateFactory _templateFactory;

        public SeparateAppDomainExecutionHost(ITextSourceProvider provider) : base(provider, new RazorCompiler())
        {
            _templateFactory = new TemplateFactory();
        }

        public static SeparateAppDomainExecutionHost Create(ITextSourceProvider textSourceProvider)
        {
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, @"C:\"));
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, @"D:\"));
            //permissions.AddPermission()

            var sn = typeof (RazorExecutionHost).Assembly.Evidence.GetHostEvidence<StrongName>();

            var domain = AppDomain.CreateDomain("Razor Sandbox", null, new AppDomainSetup { ApplicationBase = Environment.CurrentDirectory }, permissions, sn);
            
            var instance = (SeparateAppDomainExecutionHost) domain.CreateInstanceAndUnwrap(
                typeof (RazorExecutionHost).Assembly.FullName,
                typeof (SeparateAppDomainExecutionHost).FullName,
                true,
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public,
                null,
                new object[]{textSourceProvider},
                null,
                new object[0]);
            //instance._appDomain = domain;
            return instance;
        }

        protected override TemplateFactory TemplateFactory
        {
            get { return _templateFactory; }
        }
    }
}
