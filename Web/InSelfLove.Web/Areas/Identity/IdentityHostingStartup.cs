using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(InSelfLove.Web.Areas.Identity.IdentityHostingStartup))]

namespace InSelfLove.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}
