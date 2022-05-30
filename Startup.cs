using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DocumentSimilarity.Startup))]
namespace DocumentSimilarity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
