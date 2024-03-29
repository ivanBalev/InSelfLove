﻿namespace InSelfLove.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class CommentsTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;
        private readonly IJavaScriptExecutor jsExecutor;

        public CommentsTests(SeleniumServerFactory<TestStartup> server)
        {
            this.configuration = server.Configuration;
            this.server = server;
            this.browser = server.browser;

            this.jsExecutor = this.browser as IJavaScriptExecutor;

            this.browser.Manage().Window.Maximize();
        }

        private IWebElement UsernameInputField => this.browser.FindElement(By.Id("Input_Username"));

        private IWebElement PasswordInputField => this.browser.FindElement(By.Id("Input_Password"));

        private IWebElement SubmitBtn => this.browser.FindElement(By.Id("login-btn"));

        [Fact]
        public void AddArticleCommentShouldWorkCorrectly()
        {
            // Enter login data & submit
            var username = this.Login(AppConstants.UserRoleName, "Articles/test7-test7");
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText, wait);

            var latestMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));

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
            this.SeedArticleComments();

            // Return to article with comments
            var username = this.Login(AppConstants.UserRoleName, "Articles/test6-test6");

            string commentText = new string('a', 33);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            wait.Until(b => b.FindElement(By.CssSelector(".main-comment")).Displayed);
            // Get latest main comment
            var latestMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));

            // Open its reply textbox
            this.Click(latestMainComment.FindElement(By.CssSelector(".reply-button")));

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
            this.SeedArticleComments();

            var username = this.Login(AppConstants.UserRoleName, "Articles/test6-test6");
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => this.browser.FindElement(By.CssSelector(".main-comment")).Displayed);

            // Add subcomment
            string commentText = new string('b', 33);

            var showSubcommentsBtn = this.browser
                .FindElement(By.CssSelector(".main-comment"))
                .FindElement(By.CssSelector(".showSubcomments"));
            this.Click(showSubcommentsBtn);

            wait.Until(b => this.browser.FindElement(By.CssSelector(".main-comment .firstSub-comment")).Displayed);

            var firstSubComment = this.browser.FindElement(By.CssSelector(".firstSub-comment"));
            this.Click(firstSubComment.FindElement(By.CssSelector(".reply-button")));
            wait.Until(b => firstSubComment.FindElement(By.TagName("textarea")).Displayed);

            firstSubComment.FindElement(By.TagName("textarea")).SendKeys(commentText);
            this.Click(firstSubComment.FindElement(By.CssSelector(".addCommentBtn")));
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
        public void EditShouldWorkCorrectly()
        {
            this.SeedArticleComments();

            var username = this.Login(AppConstants.UserRoleName, "Articles/test6-test6");

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(By.CssSelector(".main-comment")).Displayed);

            var commentEditBtn = this.browser.FindElement(By.CssSelector(".main-comment .edit-btn"));

            this.Click(commentEditBtn);
            wait.Until(b => b.FindElement(By.CssSelector(".edit-comment")).Displayed);

            var editCommentBox = this.browser.FindElement(By.CssSelector(".edit-comment"));
            var textarea = editCommentBox.FindElement(By.CssSelector("textarea"));

            textarea.Clear();
            var editedText = "edited comment";
            textarea.SendKeys(editedText);
            this.Click(editCommentBox.FindElement(By.CssSelector(".save-edit-btn")));
            wait.Until(b => !b.FindElement(By.CssSelector(".edit-comment")).Displayed);

            var commentContent = this.browser.FindElement(By.CssSelector(".main-comment .comment-content")).Text;
            Assert.Equal(editedText, commentContent);

            this.browser.Navigate().Refresh();
            wait.Until(b => b.FindElement(By.CssSelector(".main-comment")).Displayed);

            commentContent = this.browser.FindElement(By.CssSelector(".main-comment .comment-content")).Text;
            Assert.Equal(editedText, commentContent);
        }

        [Fact]
        public void DeleteShouldWorkCorrectly()
        {
            this.SeedArticleComments();

            var username = this.Login(AppConstants.UserRoleName, "Articles/test6-test6");

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(By.CssSelector(".main-comment")).Displayed);

            var commentDeleteBtn = this.browser.FindElement(By.CssSelector(".main-comment .delete-btn"));

            this.Click(commentDeleteBtn);
            wait.Until(b => b.FindElement(By.Id("confirm-comment-delete")).Displayed);

            this.Click(this.browser.FindElement(By.CssSelector("#confirm-comment-delete .delete-comment-confirm-btn")));
            wait.Until(b => !b.FindElement(By.CssSelector("#confirm-comment-delete")).Displayed);

            Assert.Throws<NoSuchElementException>(() => this.browser.FindElement(By.CssSelector(".main-comment")));

            this.browser.Navigate().Refresh();
            Assert.Throws<NoSuchElementException>(() => this.browser.FindElement(By.CssSelector(".main-comment")));
        }

        [Fact]
        public void AddVideoCommentShouldWorkCorrectly()
        {
            var username = this.Login(AppConstants.UserRoleName, "Videos/test7-test7");
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText, wait);

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
            var username = this.Login(AppConstants.UserRoleName, "Videos/test7-test7");

            string commentText = new string('a', 33);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.AddNewMainComment(commentText, wait);

            var firstMainComment = this.browser.FindElements(By.CssSelector(".main-comment")).FirstOrDefault();
            var firstMainCommentUsername = firstMainComment.FindElement(By.CssSelector(".card-header .userName")).Text;
            var firstMainCommentText = firstMainComment.FindElement(By.CssSelector(".card-body .comment-content")).Text;

            Assert.Equal(commentText, firstMainCommentText);
            Assert.Equal(username, firstMainCommentUsername);
        }

        [Fact]
        public void AddVideoSubCommentShouldWorkCorrectly()
        {
            var username = this.Login(AppConstants.UserRoleName, "Videos/test7-test7");
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText, wait);

            // Add subcomment
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            this.Click(firstMainComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            var subCommentText = new string('a', 22);
            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(subCommentText);
            this.Click(firstMainComment.FindElement(By.CssSelector(".addCommentBtn")));

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
            var username = this.Login(AppConstants.UserRoleName, "Videos/test7-test7");
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText, wait);

            // Add subcomment
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            this.Click(firstMainComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            var subCommentText = new string('a', 22);
            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(subCommentText);
            this.Click(firstMainComment.FindElement(By.CssSelector(".addCommentBtn")));

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
            var username = this.Login(AppConstants.UserRoleName, "Videos/test7-test7");
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            string commentText = new string('a', 33);
            this.AddNewMainComment(commentText, wait);

            // Add subcomment
            var firstMainComment = this.browser.FindElement(By.CssSelector(".main-comment"));
            this.Click(firstMainComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstMainComment.FindElement(By.TagName("textarea")).Displayed);

            var subCommentText = new string('a', 22);
            firstMainComment.FindElement(By.TagName("textarea")).SendKeys(subCommentText);
            this.Click(firstMainComment.FindElement(By.CssSelector(".addCommentBtn")));

            wait.Until(b => firstMainComment.FindElements(By.CssSelector(".firstSub-comment"))
            .Where(c => c.FindElement(By.CssSelector(".userName")).Text.Equals(username) &&
            c.FindElement(By.CssSelector(".comment-content")).Text.Equals(subCommentText))
            .Count() > 0);

            var firstSubComment = this.browser.FindElement(By.CssSelector(".firstSub-comment"));
            this.Click(firstSubComment.FindElement(By.CssSelector(".reply-button")));

            wait.Until(b => firstSubComment.FindElement(By.TagName("textarea")).Displayed);

            var secondLevelSubCommentText = new string('a', 11);
            firstSubComment.FindElement(By.TagName("textarea")).SendKeys(secondLevelSubCommentText);
            this.Click(firstSubComment.FindElement(By.CssSelector(".addCommentBtn")));

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
                this.Click(btn);
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

        private void AddNewMainComment(string commentText, WebDriverWait wait)
        {
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".comment-box")));
            var addCommentBox = this.browser.FindElement(By.CssSelector(".comment-box"));
            var addCommentTextbox = addCommentBox.FindElement(By.CssSelector("[name = Content]"));
            addCommentTextbox.SendKeys(commentText);

            var submitCommentBtn = addCommentBox.FindElement(By.CssSelector(".addCommentBtn"));
            this.Click(submitCommentBtn);

            // Wait until new main comment is mounted to DOM
            wait.Until(b => b.FindElement(By.CssSelector(".main-comment")).Displayed);
        }

        private string Login(string role, string returnEndpoint)
        {
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login?ReturnUrl=/" + returnEndpoint);

            var username = this.configuration.GetSection($"{role}:Username").Value;
            var password = this.configuration.GetSection($"{role}:Password").Value;

            this.UsernameInputField.SendKeys(username);
            this.PasswordInputField.SendKeys(password);

            this.Click(this.SubmitBtn);

            return username;
        }

        private void ResetDb()
        {
            // Empty appointments collection in server
            using (var scope = this.server.Server.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Comment>>();
                var comments = repo.All().Where(x => !x.IsDeleted).ToList();

                foreach (var appt in comments)
                {
                    repo.Delete(appt);
                }

                repo.SaveChangesAsync().GetAwaiter().GetResult();
            }
        }

        private void SeedArticleComments()
        {
            using (var scope = this.server.Services.CreateScope())
            {
                var commentsRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Comment>>();
                var usersRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<ApplicationUser>>();
                var articlesRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Article>>();

                var articleId = articlesRepo.All().FirstOrDefault(a => a.Slug == "test6-test6").Id;
                var userId = usersRepo.All().FirstOrDefault(u => u.UserName == this.configuration.GetSection($"{AppConstants.UserRoleName}:Username").Value).Id;

                var comment = new Comment
                {
                    ArticleId = articleId,
                    UserId = userId,
                    Content = "comment 1",
                };

                var subcomment = new Comment
                {
                    ArticleId = articleId,
                    UserId = userId,
                    Content = "comment 2",
                };

                var subcommentLevel2 = new Comment
                {
                    ArticleId = articleId,
                    UserId = userId,
                    Content = "comment 3",
                    ParentCommentId = 2,
                };

                var subcommentLevel3 = new Comment
                {
                    ArticleId = articleId,
                    UserId = userId,
                    Content = "comment 4",
                    ParentCommentId = 2,
                };

                subcomment.SubComments = new List<Comment>
                {
                    subcommentLevel2,
                    subcommentLevel3,
                };
                comment.SubComments = new List<Comment>
                {
                    subcomment,
                };

                commentsRepo.AddAsync(comment).GetAwaiter().GetResult();
                commentsRepo.SaveChangesAsync().GetAwaiter().GetResult();
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
                this.ResetDb();
                this.browser.Manage().Cookies.DeleteAllCookies();
            }
        }
    }
}
