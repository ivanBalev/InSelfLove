namespace InSelfLove.Web.Tests
{
    using System.Linq;

    using InSelfLove.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration, IWebHostEnvironment environment)
            : base(configuration, environment)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            var descriptor = services.SingleOrDefault(
                   d => d.ServiceType ==
                       typeof(DbContextOptions<ApplicationDbContext>));

            services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        }
    }
}
