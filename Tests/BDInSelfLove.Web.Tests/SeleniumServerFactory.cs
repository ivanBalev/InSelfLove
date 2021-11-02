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

    public sealed class SeleniumServerFactory<TStartup> : CustomWebApplicationFactory<TStartup>
        where TStartup : class
    {
        public SeleniumServerFactory()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(@"appsettings.test.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            this.ClientOptions.BaseAddress = new Uri("https://localhost");
            var host = WebHost.CreateDefaultBuilder(Array.Empty<string>())
                .UseEnvironment("testing")
                .UseConfiguration(configuration)
                .UseStartup<TStartup>().Build();
            host.Start();
            this.RootUri = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.LastOrDefault();
            var testServer = new TestServer(new WebHostBuilder().UseStartup<FakeStartup>());
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
    }
}