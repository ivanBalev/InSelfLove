using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace InSelfLove.Web.Tests
{
    public class SearchPageTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {

        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;

        private const int DisplayedItemsCount = 6;
        private string testStringUri;

        public SearchPageTests(SeleniumServerFactory<TestStartup> server)
        {
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);
            this.testStringUri = server.RootUri + "/Search?searchTerm=test nonExistentText";

            this.browser.Manage().Window.Maximize();
        }

        private ReadOnlyCollection<IWebElement> PageArticles => this.browser.FindElements(By.CssSelector(".article-preview"));

        private ReadOnlyCollection<IWebElement> PageVideos => this.browser.FindElements(By.CssSelector(".video-preview"));

        private IWebElement ArticlesPagination => this.browser.FindElement(By.CssSelector("#all-articles .pagination"));

        private IWebElement VideosPagination => this.browser.FindElement(By.CssSelector("#all-videos .pagination"));

        [Fact]
        public void ArticlesAndVideosAreCorrectCountOnInitialLoad()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);

            Assert.Equal(this.PageArticles.Count, DisplayedItemsCount);
            Assert.Equal(this.PageVideos.Count, DisplayedItemsCount);

            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Search?searchTerm=hora");

            Assert.Empty(this.PageArticles);
            Assert.Equal(this.PageVideos.Count, DisplayedItemsCount);

        }

        [Fact]
        public void PaginationsHaveCorrectPagesCount()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);
            var pageLintItemsCount = 4;

            Assert.Equal(this.ArticlesPagination.FindElements(By.CssSelector(".page-link")).Count, pageLintItemsCount);
            Assert.Equal(this.VideosPagination.FindElements(By.CssSelector(".page-link")).Count, pageLintItemsCount);
        }

        [Fact]
        public void VideosHaveEqualHeights()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);

            for (int i = 1; i < this.PageVideos.Count; i++)
            {
                Assert.Equal(this.PageVideos[i].Size.Height, this.PageVideos[i - 1].Size.Height);
            }
        }

        [Fact]
        public void ArticlesHaveEqualHeights()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);

            for (int i = 1; i < this.PageArticles.Count; i++)
            {
                Assert.Equal(this.PageArticles[i].Size.Height, this.PageArticles[i - 1].Size.Height);
            }
        }

        [Fact]
        public void ArticlesPaginationWorksCorrectly()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            var jsExecutor = this.browser as IJavaScriptExecutor;

            // Get 2nd page btn
            var secondPageBtn = this.ArticlesPagination
                .FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(pl => pl.Text.Equals("2"));

            // Click on it
            jsExecutor.ExecuteScript("arguments[0].click()", secondPageBtn);

            // Wait for content to update
            wait.Until(b => this.PageArticles.Count() == 1);

            // Assert result is as expected
            Assert.Single(this.PageArticles);

            // Do the same sequence with 1st page btn
            var firstPageBtn = this.ArticlesPagination
              .FindElements(By.CssSelector(".page-link"))
              .FirstOrDefault(pl => pl.Text.Equals("1"));

            jsExecutor.ExecuteScript("arguments[0].click()", firstPageBtn);

            wait.Until(b => this.PageArticles.Count() == DisplayedItemsCount);

            Assert.Equal(DisplayedItemsCount, this.PageArticles.Count);

            // Same with nextPageBtn
            var nextPageBtn = this.ArticlesPagination
               .FindElements(By.CssSelector(".page-link"))
              .LastOrDefault();

            jsExecutor.ExecuteScript("arguments[0].click()", nextPageBtn);

            wait.Until(b => this.PageArticles.Count() == 1);

            Assert.Single(this.PageArticles);

            // And with prevPageBtn
            var prevPageBtn = this.ArticlesPagination
               .FindElements(By.CssSelector(".page-link"))
               .FirstOrDefault();

            jsExecutor.ExecuteScript("arguments[0].click()", prevPageBtn);

            wait.Until(b => this.PageArticles.Count() == DisplayedItemsCount);

            Assert.Equal(DisplayedItemsCount, this.PageVideos.Count);
        }

        [Fact]
        public void VideosPaginationWorksCorrectly()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            var jsExecutor = this.browser as IJavaScriptExecutor;

            // Get 2nd page btn
            var secondPageBtn = this.VideosPagination
                .FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(pl => pl.Text.Equals("2"));

            // Click on it
            jsExecutor.ExecuteScript("arguments[0].click()", secondPageBtn);

            // Wait for content to update
            wait.Until(b => this.PageVideos.Count() == 1);

            // Assert result is as expected
            Assert.Single(this.PageVideos);

            // Do the same sequence with 1st page btn
            var firstPageBtn = this.VideosPagination
              .FindElements(By.CssSelector(".page-link"))
              .FirstOrDefault(pl => pl.Text.Equals("1"));

            jsExecutor.ExecuteScript("arguments[0].click()", firstPageBtn);

            wait.Until(b => this.PageVideos.Count() == DisplayedItemsCount);

            Assert.Equal(DisplayedItemsCount, this.PageVideos.Count);

            // Same with nextPageBtn
            var nextPageBtn = this.VideosPagination
               .FindElements(By.CssSelector(".page-link"))
              .LastOrDefault();

            jsExecutor.ExecuteScript("arguments[0].click()", nextPageBtn);

            wait.Until(b => this.PageVideos.Count() == 1);

            Assert.Single(this.PageVideos);

            // And with prevPageBtn
            var prevPageBtn = this.VideosPagination
               .FindElements(By.CssSelector(".page-link"))
               .FirstOrDefault();

            jsExecutor.ExecuteScript("arguments[0].click()", prevPageBtn);

            wait.Until(b => this.PageVideos.Count() == DisplayedItemsCount);

            Assert.Equal(DisplayedItemsCount, this.PageVideos.Count);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.server?.Dispose();
                this.browser?.Dispose();
            }
        }
    }
}
