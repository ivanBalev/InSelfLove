namespace BDInSelfLove.Web.Tests
{
    using System;
    using System.Linq;
    using System.Threading;

    using BDInSelfLove.Common;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class CommentsTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        // Tight coupling
        private const string ArticleWithCommentsId = "6";

        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;

        public CommentsTests(SeleniumServerFactory<TestStartup> server)
        {
            this.configuration = server.Configuration;
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);

            this.browser.Manage().Window.Maximize();
            this.browser.Navigate().GoToUrl(this.server.RootUri);
        }

        private IWebElement UsernameInputField => this.browser.FindElement(By.Id("Input_Username"));

        private IWebElement PasswordInputField => this.browser.FindElement(By.Id("Input_Password"));

        private IWebElement SubmitBtn => this.browser.FindElement(By.Id("login-btn"));

        [Fact]
        public void AddCommentShouldWorkCorrectly()
        {
            this.browser.FindElements( By.CssSelector(".article-preview")).FirstOrDefault().Click();
            this.browser.FindElement(By.CssSelector("#postLogin a")).Click();
            var username = this.Login(GlobalValues.UserRoleName);

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
        public void AddSubCommentShouldWorkCorrectly()
        {
            // Click article with seeded comments
            this.browser.FindElements(By.CssSelector(".article-preview")).
                Where(a => a.GetAttribute("id").Equals(ArticleWithCommentsId))
                .FirstOrDefault()
                .Click();

            // Log in as user
            this.browser.FindElement(By.CssSelector("#postLogin a")).Click();
            var username = this.Login(GlobalValues.UserRoleName);

            // Add subcomment
            string commentText = new string('a', 33);
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            firstMainComment.FindElement(By.CssSelector(".reply-button")).Click();

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(commentText);
            firstMainComment.FindElement(By.CssSelector(".addCommentBtn")).Click();

            wait.Until(b => firstMainComment.FindElements(By.CssSelector(".firstSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(commentText))
            .Count() > 0);

            var firstSubComment = firstMainComment.FindElement(By.CssSelector(".firstSub-comment"));
            var firstSubCommentContent = firstSubComment.FindElement(By.CssSelector(".comment-content")).Text;
            var firstSubCommentAuthorUsername = firstSubComment.FindElement(By.CssSelector(".userName")).Text;

            Assert.Equal(commentText, firstSubCommentContent);
            Assert.Equal(username, firstSubCommentAuthorUsername);
        }

        [Fact]
        public void AddSecondLevelSubCommentShouldWorkCorrectly()
        {
            // Click article with seeded comments
            this.browser.FindElements(By.CssSelector(".article-preview")).
                Where(a => a.GetAttribute("id").Equals(ArticleWithCommentsId))
                .FirstOrDefault()
                .Click();

            // Log in as user
            this.browser.FindElement(By.CssSelector("#postLogin a")).Click();
            var username = this.Login(GlobalValues.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Add subcomment
            string commentText = new string('b', 33);
            this.browser.FindElement(By.CssSelector(".main-comment"))
                .FindElement(By.CssSelector(".showSubcomments ")).Click();
            wait.Until(b => this.browser.FindElement(By.CssSelector(".main-comment .firstSub-comment")).Displayed);

            var firstSubComment = this.browser.FindElement(By.CssSelector(".firstSub-comment"));
            firstSubComment.FindElement(By.CssSelector(".reply-button")).Click();

            wait.Until(b => firstSubComment.FindElement(By.TagName("textarea")).Displayed);

            firstSubComment.FindElement(By.TagName("textarea")).SendKeys(commentText);
            firstSubComment.FindElement(By.CssSelector(".addCommentBtn")).Click();

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
                btn.Click();
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

        private void AddNewMainComment(string commentText)
        {
            var addCommentBox = this.browser.FindElement(By.CssSelector(".comment-box"));
            var addCommentTextbox = addCommentBox.FindElement(By.CssSelector("[name = Content]"));
            addCommentTextbox.SendKeys(commentText);

            var submitComment = addCommentBox.FindElement(By.CssSelector(".addCommentBtn"));
            submitComment.Click();
        }

        private string Login(string role)
        {
            var username = this.configuration.GetSection($"{role}:Username").Value;
            var password = this.configuration.GetSection($"{role}:Password").Value;

            this.UsernameInputField.SendKeys(username);
            this.PasswordInputField.SendKeys(password);
            this.SubmitBtn.Click();
            this.WaitForAjax();

            return username;
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
