﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Xunit;

namespace BDInSelfLove.Web.Tests
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

            this.browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
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

            this.ArticlesPagination.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(pl => pl.Text.Equals("2"))
                .Click();

            this.WaitForAjax();
            Assert.Single(this.PageArticles);

            this.ArticlesPagination.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(pl => pl.Text.Equals("1"))
                .Click();

            this.WaitForAjax();
            Assert.Equal(this.PageArticles.Count, DisplayedItemsCount);

            this.ArticlesPagination.FindElements(By.CssSelector(".page-link"))
                .LastOrDefault()
                .Click();

            this.WaitForAjax();
            Assert.Single(this.PageArticles);

            this.ArticlesPagination.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault()
                .Click();

            this.WaitForAjax();
            Assert.Equal(this.PageArticles.Count, DisplayedItemsCount);
        }

        [Fact]
        public void VideosPaginationWorksCorrectly()
        {
            this.browser.Navigate().GoToUrl(this.testStringUri);

            this.VideosPagination.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(pl => pl.Text.Equals("2"))
                .Click();

            this.WaitForAjax();
            Assert.Single(this.PageVideos);

            this.VideosPagination.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(pl => pl.Text.Equals("1"))
                .Click();

            this.WaitForAjax();
            Assert.Equal(this.PageVideos.Count, DisplayedItemsCount);

            this.VideosPagination.FindElements(By.CssSelector(".page-link"))
                .LastOrDefault()
                .Click();

            this.WaitForAjax();
            Assert.Single(this.PageVideos);

            this.VideosPagination.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault()
                .Click();

            this.WaitForAjax();
            Assert.Equal(this.PageVideos.Count, DisplayedItemsCount);
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

        private void WaitForAjax()
        {
            while (true)
            {
                if ((bool)((IJavaScriptExecutor)this.browser)
                .ExecuteScript("return jQuery.active == 0"))
                {
                    break;
                }

                Thread.Sleep(500);
            }
        }
    }
}