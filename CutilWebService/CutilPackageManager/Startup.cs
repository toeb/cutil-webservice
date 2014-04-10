using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CutilPackageManager.Startup))]
namespace CutilPackageManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
      
    }
}
