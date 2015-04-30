using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PictureGallery.Startup))]
namespace PictureGallery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
