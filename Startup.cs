using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SamsLiquorShop.Startup))]
namespace SamsLiquorShop
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
