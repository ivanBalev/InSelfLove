namespace InSelfLove.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Videos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    using Xunit;

    public class HomePageTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private const int ArticlesCount = 5;
        private const int VideosCount = 5;

        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;

        public HomePageTests(SeleniumServerFactory<TestStartup> server)
        {
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);

            this.browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            this.browser.Manage().Window.Maximize();
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Home/Contacts");
        }

        private ReadOnlyCollection<IWebElement> PageArticles => this.browser.FindElements(By.CssSelector(".article-preview"));

        private ReadOnlyCollection<IWebElement> PageVideos => this.browser.FindElements(By.CssSelector(".carousel-item"));

        [Fact]
        public async Task ArticlesAndVideosAreArrangedCorrectly()
        {
            List<Article> dbArticles = new List<Article>();
            List<Video> dbVideos = new List<Video>();

            // Take last videos and articles
            using (var scope = this.server.Server.Host.Services.CreateScope())
            {
                var articleService = scope.ServiceProvider.GetRequiredService<IArticleService>();
                var videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();
                dbArticles = await articleService.GetAll(ArticlesCount).ToListAsync();
                dbVideos = await videoService.GetAll(VideosCount).ToListAsync();
            }

            // Determine which is latest
            var latestArticle = dbArticles[0];
            var latestVideo = dbVideos[0];
            bool videoIsLatest = DateTime.Compare(latestArticle.CreatedOn, latestVideo.CreatedOn) == -1;

            // Gather data from screen
            var featuredItemTitle = this.browser.FindElement(By.CssSelector(".showcase-wrapper h2")).Text;

            // Assert data is same as db data
            foreach (var pageArticle in this.PageArticles)
            {
                Assert.Contains(dbArticles, a => a.Id.ToString().Equals(pageArticle.GetAttribute("id")));
            }

            foreach (var pageVideo in this.PageVideos)
            {
                Assert.Contains(dbVideos, v => v.Id.ToString().Equals(pageVideo.GetAttribute("id")));
            }

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
