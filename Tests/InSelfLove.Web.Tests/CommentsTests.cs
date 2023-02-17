namespace InSelfLove.Web.Tests
{
    using System;
    using System.Linq;

    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class CommentsTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        // TODO: Remove tight coupling
        private const string ArticleWithCommentsId = "6";

        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;
        private readonly IJavaScriptExecutor jsExecutor;

        public CommentsTests(SeleniumServerFactory<TestStartup> server)
        {
            this.configuration = server.Configuration;
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);
            this.jsExecutor = this.browser as IJavaScriptExecutor;

            this.browser.Manage().Window.Maximize();
            this.browser.Navigate().GoToUrl(this.server.RootUri);
        }

        private IWebElement UsernameInputField => this.browser.FindElement(By.Id("Input_Username"));

        private IWebElement PasswordInputField => this.browser.FindElement(By.Id("Input_Password"));

        private IWebElement SubmitBtn => this.browser.FindElement(By.Id("login-btn"));

        [Fact]
        public void AddArticleCommentShouldWorkCorrectly()
        {
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Click latest article
            this.Click(this.browser.FindElements(By.CssSelector(".article-preview a")).FirstOrDefault());

            // Click login btn on single article page
            this.Click(this.browser.FindElement(By.CssSelector("#postLogin a")));

            // Enter login data & submit
            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText);

            // Wait until new main comment is mounted to DOM
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".main-comment")));

            var latestMainComment = this.browser.FindElements(By.CssSelector(".main-comment")).FirstOrDefault();

            // Extract username & content from latest comment
            var latestMainCommentUsername = latestMainComment.FindElement(By.CssSelector(".card-header .userName")).Text;
            var latestMainCommentText = latestMainComment.FindElement(By.CssSelector(".card-body .comment-content")).Text;

            // Assert username & content match that of the comment we just added
            Assert.Equal(commentText, latestMainCommentText);
            Assert.Equal(username, latestMainCommentUsername);
        }

        [Fact]
        public void AddArticleSubCommentShouldWorkCorrectly()
        {
            // Click article with seeded comments
            // id is attached to .article-preview div so we target that first
            // then we target the anchor inside that div which we need to click
            this.Click(this.browser.FindElements(By.CssSelector(".article-preview"))
                .Where(a => a.GetAttribute("id").Equals(ArticleWithCommentsId))
                .FirstOrDefault().FindElement(By.CssSelector("a")));

            // Log in as user
            this.Click(this.browser.FindElement(By.CssSelector("#postLogin a")));
            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);

            // Get latest main comment
            var latestMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));

            // Open its reply textbox
            this.Click(latestMainComment.FindElement(By.CssSelector(".reply-button")));

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => latestMainComment.FindElement(By.TagName("textarea")).Displayed);

            // Once the reply textbox is displayed, enter comment text inside and submit
            latestMainComment.FindElement(By.TagName("textarea")).SendKeys(commentText);
            this.Click(latestMainComment.FindElement(By.CssSelector(".addCommentBtn")));

            // Wait until the comment we just submitted is mounted
            wait.Until(b => latestMainComment.FindElements(By.CssSelector(".firstSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(commentText))
            .Count() > 0);

            var firstSubComment = latestMainComment.FindElement(By.CssSelector(".firstSub-comment"));

            // Get that comment's content & username
            var firstSubCommentContent = firstSubComment.FindElement(By.CssSelector(".comment-content")).Text;
            var firstSubCommentAuthorUsername = firstSubComment.FindElement(By.CssSelector(".userName")).Text;

            // Assert they match our entered data
            Assert.Equal(commentText, firstSubCommentContent);
            Assert.Equal(username, firstSubCommentAuthorUsername);
        }

        [Fact]
        public void AddArticleSecondLevelSubCommentShouldWorkCorrectly()
        {
            // TODO: Go directly to the article (get slug from db and append to root url)

            // Click article with seeded comments
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(
                    By.CssSelector(".article-preview"))
                    .Where(a => a.GetAttribute("id").Equals(ArticleWithCommentsId))
                    .FirstOrDefault()
                    .FindElement(By.CssSelector("a")));

            // Log in as user
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElement(By.CssSelector("#postLogin a")));
            var username = this.Login(AppConstants.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Add subcomment
            string commentText = new string('b', 33);
            var showSubcommentsBtn = this.browser
                .FindElement(By.CssSelector(".main-comment"))
                .FindElement(By.CssSelector(".showSubcomments"));
            this.jsExecutor.ExecuteScript("arguments[0].click()", showSubcommentsBtn);

            wait.Until(b => this.browser.FindElement(By.CssSelector(".main-comment .firstSub-comment")).Displayed);

            var firstSubComment = this.browser.FindElement(By.CssSelector(".firstSub-comment"));
            this.jsExecutor.ExecuteScript("arguments[0].click()", firstSubComment.FindElement(By.CssSelector(".reply-button")));
            wait.Until(b => firstSubComment.FindElement(By.TagName("textarea")).Displayed);

            firstSubComment.FindElement(By.TagName("textarea")).SendKeys(commentText);
            this.jsExecutor.ExecuteScript("arguments[0].click()", firstSubComment.FindElement(By.CssSelector(".addCommentBtn")));
            wait.Until(b => firstSubComment.FindElements(By.CssSelector(".secondSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(commentText))
            .Count() > 0);

            var secondSubcomment = firstSubComment.FindElement(By.CssSelector(".secondSub-comment"));
            var secondSubommentContent = secondSubcomment.FindElement(By.CssSelector(".comment-content")).Text;
            var secondSubcommentAuthorUsername = secondSubcomment.FindElement(By.CssSelector(".userName")).Text;

            Assert.Equal(commentText, secondSubommentContent);
            Assert.Equal(username, secondSubcommentAuthorUsername);

            this.browser.Navigate().Refresh();

            var subcommentBtns = this.browser.FindElements(By.CssSelector(".showSubcomments")).ToList();
            foreach (var btn in subcommentBtns)
            {
                this.jsExecutor.ExecuteScript("arguments[0].click()", btn);
            }

            var secondSubcommentPostRefresh = this.browser.FindElement(
                By.CssSelector(".firstSub-comment .secondSub-comment"));
            var secondSubommentPostRefreshContent = secondSubcommentPostRefresh.FindElement(
                By.CssSelector(".comment-content")).Text;
            var secondSubcommentPostRefreshAuthorUsername = secondSubcommentPostRefresh.FindElement(
                By.CssSelector(".userName")).Text;

            Assert.Equal(commentText, secondSubommentPostRefreshContent);
            Assert.Equal(username, secondSubcommentPostRefreshAuthorUsername);
        }

        [Fact]
        public void AddVideoCommentShouldWorkCorrectly()
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector("a[href='/Videos']")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector(".video-preview a")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElement(By.CssSelector("#postLogin a")));

            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".main-comment")));

            var firstMainComment = this.browser.FindElements(By.CssSelector(".main-comment")).FirstOrDefault();
            var firstMainCommentUsername = firstMainComment.FindElement(By.CssSelector(".card-header .userName")).Text;
            var firstMainCommentText = firstMainComment.FindElement(By.CssSelector(".card-body .comment-content")).Text;

            Assert.Equal(commentText, firstMainCommentText);
            Assert.Equal(username, firstMainCommentUsername);
        }

        [Fact]
        public void AddVideoCommentShouldWorkCorrectlyAfterRefresh()
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector("a[href='/Videos']")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector(".video-preview a")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElement(By.CssSelector("#postLogin a")));

            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".main-comment")));

            this.browser.Navigate().GoToUrl(this.browser.Url);

            var firstMainComment = this.browser.FindElements(By.CssSelector(".main-comment")).FirstOrDefault();
            var firstMainCommentUsername = firstMainComment.FindElement(By.CssSelector(".card-header .userName")).Text;
            var firstMainCommentText = firstMainComment.FindElement(By.CssSelector(".card-body .comment-content")).Text;

            Assert.Equal(commentText, firstMainCommentText);
            Assert.Equal(username, firstMainCommentUsername);
        }

        [Fact]
        public void AddVideoSubCommentShouldWorkCorrectly()
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector("a[href='/Videos']")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector(".video-preview a")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElement(By.CssSelector("#postLogin a")));

            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".main-comment")));

            // Add subcomment
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstMainComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            var subCommentText = new string('a', 22);
            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(subCommentText);
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstMainComment.FindElement(By.CssSelector(".addCommentBtn")));

            wait.Until(b => firstMainComment.FindElements(By.CssSelector(".firstSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(subCommentText))
            .Count() > 0);

            var firstSubComment = firstMainComment.FindElement(By.CssSelector(".firstSub-comment"));
            var firstSubCommentContent = firstSubComment.FindElement(By.CssSelector(".comment-content")).Text;
            var firstSubCommentAuthorUsername = firstSubComment.FindElement(By.CssSelector(".userName")).Text;

            Assert.Equal(subCommentText, firstSubCommentContent);
            Assert.Equal(username, firstSubCommentAuthorUsername);
        }

        [Fact]
        public void AddVideoSubCommentShouldWorkCorrectlyAfterRefresh()
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector("a[href='/Videos']")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector(".video-preview a")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElement(By.CssSelector("#postLogin a")));

            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".main-comment")));

            this.browser.Navigate().GoToUrl(this.browser.Url);

            // Add subcomment
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstMainComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            var subCommentText = new string('a', 22);
            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(subCommentText);
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstMainComment.FindElement(By.CssSelector(".addCommentBtn")));

            wait.Until(b => firstMainComment.FindElements(By.CssSelector(".firstSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(subCommentText))
            .Count() > 0);

            var firstSubComment = firstMainComment.FindElement(By.CssSelector(".firstSub-comment"));
            var firstSubCommentContent = firstSubComment.FindElement(By.CssSelector(".comment-content")).Text;
            var firstSubCommentAuthorUsername = firstSubComment.FindElement(By.CssSelector(".userName")).Text;

            Assert.Equal(subCommentText, firstSubCommentContent);
            Assert.Equal(username, firstSubCommentAuthorUsername);
        }

        [Fact]
        public void AddVideoSecondLevelSubCommentShouldWorkCorrectly()
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector("a[href='/Videos']")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElements(By.CssSelector(".video-preview a")).FirstOrDefault());
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                this.browser.FindElement(By.CssSelector("#postLogin a")));

            var username = this.Login(AppConstants.UserRoleName);

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".main-comment")));

            // Add subcomment
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstMainComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            var subCommentText = new string('a', 22);
            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(subCommentText);
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstMainComment.FindElement(By.CssSelector(".addCommentBtn")));

            wait.Until(b => firstMainComment.FindElements(By.CssSelector(".firstSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(subCommentText))
            .Count() > 0);

            var firstSubComment = this.browser.FindElement(By.CssSelector(".firstSub-comment"));
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstSubComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstSubComment.FindElement(By.TagName("textarea")).Displayed);

            var secondLevelSubCommentText = new string('a', 11);
            firstSubComment.FindElement(By.TagName("textarea")).SendKeys(secondLevelSubCommentText);
            this.jsExecutor.ExecuteScript("arguments[0].click()",
                firstSubComment.FindElement(By.CssSelector(".addCommentBtn")));

            wait.Until(b => firstSubComment.FindElements(By.CssSelector(".secondSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(secondLevelSubCommentText))
            .Count() > 0);

            var secondSubcomment = firstSubComment.FindElement(By.CssSelector(".secondSub-comment"));
            var secondSubommentContent = secondSubcomment.FindElement(By.CssSelector(".comment-content")).Text;
            var secondSubcommentAuthorUsername = secondSubcomment.FindElement(By.CssSelector(".userName")).Text;

            Assert.Equal(secondLevelSubCommentText, secondSubommentContent);
            Assert.Equal(username, secondSubcommentAuthorUsername);

            this.browser.Navigate().Refresh();

            wait.Until(b => b.FindElements(By.CssSelector(".showSubcomments")).Count > 0);
            var subcommentBtns = this.browser.FindElements(By.CssSelector(".showSubcomments")).ToList();
            foreach (var btn in subcommentBtns)
            {
                this.jsExecutor.ExecuteScript("arguments[0].click()", btn);
            }

            var secondSubcommentPostRefresh = this.browser.FindElement(
                By.CssSelector(".firstSub-comment .secondSub-comment"));
            var secondSubommentPostRefreshContent = secondSubcommentPostRefresh.FindElement(
                By.CssSelector(".comment-content")).Text;
            var secondSubcommentPostRefreshAuthorUsername = secondSubcommentPostRefresh.FindElement(
                By.CssSelector(".userName")).Text;

            Assert.Equal(secondLevelSubCommentText, secondSubommentPostRefreshContent);
            Assert.Equal(username, secondSubcommentPostRefreshAuthorUsername);
        }

        private void Click(IWebElement element)
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()", element);
        }

        private void AddNewMainComment(string commentText)
        {
            var addCommentBox = this.browser.FindElement(By.CssSelector(".comment-box"));
            var addCommentTextbox = addCommentBox.FindElement(By.CssSelector("[name = Content]"));
            addCommentTextbox.SendKeys(commentText);

            var submitCommentBtn = addCommentBox.FindElement(By.CssSelector(".addCommentBtn"));
            this.Click(submitCommentBtn);
        }

        private string Login(string role)
        {
            var username = this.configuration.GetSection($"{role}:Username").Value;
            var password = this.configuration.GetSection($"{role}:Password").Value;

            this.UsernameInputField.SendKeys(username);
            this.PasswordInputField.SendKeys(password);

            this.Click(this.SubmitBtn);

            return username;
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
