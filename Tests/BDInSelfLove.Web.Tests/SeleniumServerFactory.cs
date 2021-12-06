namespace BDInSelfLove.Web.Tests
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class SeleniumServerFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile(@"appsettings.test.json", false, false)
             .AddEnvironmentVariables()
             .Build();

        public SeleniumServerFactory()
        {
            this.ClientOptions.BaseAddress = new Uri("https://localhost");
            var host = WebHost.CreateDefaultBuilder(Array.Empty<string>())
                .UseConfiguration(this.configuration)
                .UseEnvironment("testing")
                .UseStartup<TStartup>()
                .Build();

            host.Start();

            this.RootUri = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.LastOrDefault();

            var testServer = new TestServer(new WebHostBuilder()
                .UseStartup<FakeStartup>());
        }

        public string RootUri { get; set; }

        public class FakeStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure()
            {
            }
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(null)
                          .UseStartup<TStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".")
            .UseConfiguration(this.configuration)
            .UseEnvironment("testing")
            .UseStartup<TStartup>();
            base.ConfigureWebHost(builder);
        }

        //private void ConfigureInMemoryDb(IServiceCollection services)
        //{
        //    //builder.ConfigureServices(services =>
        //    //{
        //        var descriptor = services.SingleOrDefault(
        //            d => d.ServiceType ==
        //                typeof(DbContextOptions<ApplicationDbContext>));

        //        services.Remove(descriptor);

        //        services.AddDbContext<ApplicationDbContext>(options =>
        //        {
        //            options.UseInMemoryDatabase("InMemoryDbForTesting");
        //        });

        //        //var sp = services.BuildServiceProvider();

        //        //using (var scope = sp.CreateScope())
        //        //{
        //        //    var scopedServices = scope.ServiceProvider;
        //        //    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
        //        //    var logger = scopedServices
        //        //        .GetRequiredService<ILogger<SeleniumServerFactory<TStartup>>>();

        //        //    db.Database.EnsureCreated();

        //        //    try
        //        //    {
        //        //        Utilities.InitializeDbForTests(db);
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        logger.LogError(ex, "An error occurred seeding the " +
        //        //            "database with test messages. Error: {Message}", ex.Message);
        //        //    }
        //        //}
        //    //});
        //}
    }
}
