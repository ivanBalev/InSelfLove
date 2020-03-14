using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BDInSelfLove.Web.Areas.Identity.IdentityHostingStartup))]

namespace BDInSelfLove.Web.Areas.Identity
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
