namespace InSelfLove.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Web.Controllers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    using Xunit;

    public class HomePageTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;

        private readonly int articlesCount = HomeController.IndexArticlesCount;
        private readonly int aideosCount = HomeController.IndexVideosCount;

        public HomePageTests(SeleniumServerFactory<TestStartup> server)
        {
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);

            this.browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            this.browser.Manage().Window.Maximize();
            this.browser.Navigate().GoToUrl(this.server.RootUri);
        }

        private ReadOnlyCollection<IWebElement> PageArticles => this.browser.FindElements(By.CssSelector(".article-preview"));

        private ReadOnlyCollection<IWebElement> PageVideos => this.browser.FindElements(By.CssSelector(".carousel-item"));

        [Fact]
        public async Task ArticlesAndVideosAreArrangedCorrectly()
        {
            List<Article> dbArticles = new List<Article>();
            List<Video> dbVideos = new List<Video>();

            // Take last videos and articles from database
            using (var scope = this.server.Server.Host.Services.CreateScope())
            {
                // Get services
                var articleService = scope.ServiceProvider.GetRequiredService<IArticleService>();
                var videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();

                // Retrieve entities
                dbArticles = await articleService.GetAll(this.articlesCount).ToListAsync();
                dbVideos = await videoService.GetAll(this.aideosCount).ToListAsync();
            }

            // Assert articles are retrieved correctly
            foreach (var pageArticle in this.PageArticles)
            {
                Assert.Contains(dbArticles, a => a.Id.ToString().Equals(pageArticle.GetAttribute("id")));
            }

            // Assert videos are retrieved correctly
            foreach (var pageVideo in this.PageVideos)
            {
                Assert.Contains(dbVideos, v => v.Id.ToString().Equals(pageVideo.GetAttribute("id")));
            }

            // Determine whether last post is article or video
            var latestArticle = dbArticles[0];
            var latestVideo = dbVideos[0];
            bool videoIsLatest = DateTime.Compare(latestArticle.CreatedOn, latestVideo.CreatedOn) == -1;

            // Get latest (featured) post from page
            var featuredItemTitle = this.browser.FindElement(By.CssSelector(".showcase-wrapper h2")).Text;

            // Assert featured item is correct
            if (videoIsLatest)
            {
                Assert.Equal(latestVideo.Title, featuredItemTitle);
            }
            else
            {
                Assert.Equal(latestArticle.Title, featuredItemTitle);
            }
        }

        [Fact]
        public void ArticleCardsHaveEqualHeights()
        {
            // TODO: check contacts preview has same height as article previews

            // Assert js script for equalizing article preview cards' heights works
            for (int i = 1; i < this.PageArticles.Count; i++)
            {
                Assert.Equal(this.PageArticles[i].Size.Height, this.PageArticles[i - 1].Size.Height);
            }
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
