namespace InSelfLove.Web.Tests
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;

    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class AppointmentPageUserTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private const string ApprovedText = "✓";
        private const string AwaitingApprovalText = "\u27F3";
        private const string AvailableText = "+";

        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;

        public AppointmentPageUserTests(SeleniumServerFactory<TestStartup> server)
        {
            this.configuration = server.Configuration;
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);

            this.browser.Manage().Window.Maximize();
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/appointments");
        }

        private IWebElement UsernameInputField => this.browser.FindElement(By.Id("Input_Username"));

        private IWebElement PasswordInputField => this.browser.FindElement(By.Id("Input_Password"));

        private IWebElement SubmitBtn => this.browser.FindElement(By.Id("login-btn"));

        private By AppointmentDetailsModalSelector => By.Id("appointmentDetailsModal");

        private By AppointmentSelector => By.CssSelector(".fc-event");

        private By CancelAppointmentBtnSelector => By.Id("btnDelete");

        private By CancelAppointmentConfirmModalSelector => By.Id("cancelAppointmentConfirm");

        private By BookAppointmentModalSelector => By.Id("bookAppointmentModal");

        [Fact]
        public void AppointmentBookingWorksCorrectly()
        {
            var username = this.Login(AppConstants.UserRoleName);
            this.WaitForAjax();

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            var appointmentLocation = this.BookAppointment(wait);

            // Assert Book appointment modal is hidden
            var bookAppointmentModalPostUpdate = this.browser.FindElement(this.BookAppointmentModalSelector);
            var bookAppointmentModalStyle = bookAppointmentModalPostUpdate.GetAttribute("style").Trim();

            var displayNoneText = "display: none;";
            Assert.Equal(displayNoneText, bookAppointmentModalStyle);

            // Click on same appointment
            Actions actions = new Actions(this.browser);
            actions.MoveByOffset(
                           appointmentLocation.X + 10, appointmentLocation.Y + 10)
                           .Click().Perform();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Check if it's now awaiting approval for current user
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);
            var usernameOnDetailsModal = appointmentDetailsModal.FindElement(
                By.CssSelector(".usernameGroup .username")).Text;

            Assert.Equal(username, usernameOnDetailsModal);
        }

        [Fact]
        public void PendingAppointmentCancellation()
        {
            this.Login(AppConstants.UserRoleName);
            this.WaitForAjax();

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            Actions actions = new Actions(this.browser);

            // Get pending appointment and save its location
            var appointmentPendingApproval = this.browser.FindElements(this.AppointmentSelector)
                .Where(e => e.Text.Equals(AwaitingApprovalText))
                .FirstOrDefault();
            var pendingApprovalElementLocation = appointmentPendingApproval.Location;

            // Open appointment details modal
            appointmentPendingApproval.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Cancel appointment
            var cancelAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                this.CancelAppointmentBtnSelector);
            cancelAppointmentBtn.Click();
            this.WaitForAjax();

            // Confirm cancellation
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            cancelConfirmBtn.Click();
            this.WaitForAjax();

            // Click on same element
            actions.MoveByOffset(
                pendingApprovalElementLocation.X + 10, pendingApprovalElementLocation.Y + 10)
                .Click().Perform();
            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);

            // Assert book appointment modal is now displayed
            Assert.True(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);
        }

        [Fact]
        public void ApprovedAppointmentCancellation()
        {
            this.Login(AppConstants.UserRoleName);
            this.WaitForAjax();

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            Actions actions = new Actions(this.browser);

            // Get pending appointment and save its location
            var approvedAppointment = this.browser.FindElements(this.AppointmentSelector)
                .Where(e => e.Text.Equals(ApprovedText))
                .FirstOrDefault();
            var approvedElementLocation = approvedAppointment.Location;

            // Open appointment details modal
            approvedAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Cancel appointment
            var cancelAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                this.CancelAppointmentBtnSelector);
            cancelAppointmentBtn.Click();
            this.WaitForAjax();

            // Confirm cancellation
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            cancelConfirmBtn.Click();
            this.WaitForAjax();

            // Click on same element
            actions.MoveByOffset(
                approvedElementLocation.X + 10, approvedElementLocation.Y + 10)
                .Click().Perform();
            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);

            // Assert book appointment modal is now displayed
            Assert.True(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);
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

        private Point BookAppointment(WebDriverWait wait)
        {
            // Open first available appointment
            var availableAppointment = this.browser.FindElements(this.AppointmentSelector)
                .Where(e => e.Text.Equals(AvailableText)).FirstOrDefault();
            availableAppointment.Click();
            var availableAppointmentElementLocation = availableAppointment.Location;

            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);
            var bookAppointmentModal = this.browser.FindElement(this.BookAppointmentModalSelector);

            // Populate issue description
            var issueDescriptionTextbox = bookAppointmentModal.FindElement(By.Id("patientIssueDescription"));
            issueDescriptionTextbox.SendKeys(new string('a', 31));

            // Click book appointment btn
            var sendAppointmentBtn = bookAppointmentModal.FindElement(By.Id("sendAppointment"));
            sendAppointmentBtn.Click();
            this.WaitForAjax();

            return availableAppointmentElementLocation;
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
