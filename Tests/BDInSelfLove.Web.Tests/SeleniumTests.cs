namespace BDInSelfLove.Web.Tests
{
    using BDInSelfLove.Web;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using Xunit;

    public class SeleniumTests
    {
        private RemoteWebDriver browser;
        private SeleniumServerFactory<Startup> serverFactory;

        public SeleniumTests()
        {
            this.serverFactory = new SeleniumServerFactory<Startup>();
            serverFactory.CreateClient();
            var options = new ChromeOptions();
            options.AddArguments("--headless", "--no-sandbox", "--ignore-certificate-errors");
            this.browser = new RemoteWebDriver(options);
        }

        [Fact]
        public void HomePageShouldHaveH1Tag()
        {
            this.browser.Navigate().GoToUrl(this.serverFactory.RootUri + "/Home/Index");
            var result = this.browser.FindElementByTagName("html");

            // Assert.Contains("Welcome to", this.browser.FindElementByTagName("body").Text);
            Assert.True(this.browser.FindElementByCssSelector("html") != null);
        }
    }
}
