using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FacturadorTaller.Startup))]
namespace FacturadorTaller
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
