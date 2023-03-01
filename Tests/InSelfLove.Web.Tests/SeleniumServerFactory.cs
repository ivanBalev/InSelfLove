namespace InSelfLove.Web.Tests
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
    using Microsoft.Extensions.Hosting;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Safari;

    public sealed class SeleniumServerFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile(@"appsettings.test.json", false, false)
             .AddEnvironmentVariables()
             .Build();

        public readonly IWebDriver browser;

        public SeleniumServerFactory()
        {
            this.ClientOptions.BaseAddress = new Uri("https://localhost");
            var host = WebHost.CreateDefaultBuilder(Array.Empty<string>())
                .UseConfiguration(this.configuration)
                .UseEnvironment("Test")
                .UseStartup<TStartup>()
                .Build();

            host.Start();

            this.RootUri = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.LastOrDefault();

            var browserName = this.configuration.GetSection("Browser:Name").Value;
            switch (browserName)
            {
                case "chrome":
                    var opts = new ChromeOptions();
                    opts.AcceptInsecureCertificates = true;
                    this.browser = new ChromeDriver(opts);
                    break;
                case "firefox":
                    var firefoxOpts = new FirefoxOptions();
                    firefoxOpts.AcceptInsecureCertificates = true;
                    this.browser = new FirefoxDriver(firefoxOpts);
                    break;
                case "safari":
                    var safariOpts = new SafariOptions();
                    safariOpts.AcceptInsecureCertificates = true;
                    this.browser = new SafariDriver(safariOpts);
                    break;
            }
        }

        public IConfiguration Configuration => this.configuration;

        public string RootUri { get; set; }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(null)
                          .UseStartup<TStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
            .UseContentRoot(".")
            .UseConfiguration(this.configuration)
            .UseEnvironment("Test")
            .UseStartup<TStartup>();
            base.ConfigureWebHost(builder);
        }
    }
}
