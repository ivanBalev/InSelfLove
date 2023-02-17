namespace InSelfLove.Web.Tests
{
    using System;
    using System.Linq;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class AppointmentPageTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;
        private readonly IJavaScriptExecutor jsExecutor;

        public AppointmentPageTests(SeleniumServerFactory<TestStartup> server)
        {
            this.configuration = server.Configuration;
            this.server = server;
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            this.browser = new ChromeDriver(opts);
            this.jsExecutor = this.browser as IJavaScriptExecutor;

            this.browser.Manage().Window.Maximize();
        }

        private IWebElement UsernameInputField => this.browser.FindElement(By.Id("Input_Username"));

        private IWebElement PasswordInputField => this.browser.FindElement(By.Id("Input_Password"));

        private IWebElement SubmitBtn => this.browser.FindElement(By.Id("login-btn"));

        private By AppointmentDetailsModalSelector => By.Id("appointmentDetailsModal");

        private By CalendarDaySelector => By.CssSelector(".fc-daygrid-day-events");

        private By AppointmentSelector => By.CssSelector(".fc-event");

        private By DailyAvailabilityModalSelector => By.Id("dailyAvailabilityModal");

        private By DailyTimeSlotSelector => By.CssSelector(".dailyTimeSlot");

        private By SubmitDailyAvailabilityBtnSelector => By.Id("submitDailyAvailability");

        private By CancelAppointmentBtnSelector => By.Id("btnDelete");

        private By CancelAppointmentConfirmModalSelector => By.Id("cancelAppointmentConfirm");

        // Admin tests
        [Fact]
        public void AppointmentCreationWorksCorrectly()
        {
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login");
            this.Login(AppConstants.AdministratorRoleName);
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/appointments");

            Actions actions = new Actions(this.browser);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            wait.Until(b => b.FindElement(this.CalendarDaySelector).Displayed);

            // Open daily availability modal
            var lastDayOnCalendarDisplay = this.browser.FindElements(this.CalendarDaySelector).LastOrDefault();
            actions.MoveToElement(lastDayOnCalendarDisplay, 30, 30).Click().Perform();
            wait.Until(b => b.FindElement(this.DailyAvailabilityModalSelector).Displayed);
            var dailyModal = this.browser.FindElement(this.DailyAvailabilityModalSelector);

            // Select a slot and submit
            var firstSlot = dailyModal.FindElement(this.DailyTimeSlotSelector);
            var firstSlotTime = firstSlot.Text.Trim(' ', '0').Split(':')[0];
            this.Click(firstSlot);
            var submitDailyAvailabilityBtn = dailyModal.FindElement(this.SubmitDailyAvailabilityBtnSelector);
            this.Click(submitDailyAvailabilityBtn);

            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);
            // Confirm appointment has been created (other same-day appointments have been deleted by design)
            var lastAppointment = this.browser.FindElements(this.AppointmentSelector).LastOrDefault();
            this.Click(lastAppointment);
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            var lastAppointmentStartTime = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(By.CssSelector(".start")).Text;
            Assert.Equal(firstSlotTime, lastAppointmentStartTime.Trim(' ', '0').Split(':')[0]);
        }

        [Fact]
        public void AvailableAppointmentCancellationWorksCorrectly()
        {
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login");
            this.Login(AppConstants.AdministratorRoleName);
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/appointments");

            // Open first appointment details
            var firstAppointment = this.browser.FindElement(this.AppointmentSelector);
            this.Click(firstAppointment);

            // Extract date and start time
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);

            var appointmentDateString = appointmentDetailsModal.FindElement(By.CssSelector(".date")).Text;
            var appointmentStartTime = appointmentDetailsModal.FindElement(By.CssSelector(".start")).Text;

            // Delete appointment
            var btnDelete = this.browser.FindElement(this.CancelAppointmentBtnSelector);
            this.Click(btnDelete);

            // Confirm deletion
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            Actions actions = new Actions(this.browser);
            actions.MoveToElement(cancelConfirmBtn).Click().Perform();

            wait.Until(b => !b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);
            // Open new first appointment details
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);
            var newFirstAppointment = this.browser.FindElement(this.AppointmentSelector);
            this.Click(newFirstAppointment);
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            var newAppointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);

            // Extract date and start time
            var newFirstAppointmentDateString = newAppointmentDetailsModal.FindElement(By.CssSelector(".date")).Text;
            var newFirstppointmentStartTime = newAppointmentDetailsModal.FindElement(By.CssSelector(".start")).Text;

            // Assert old appointment has been deleted
            Assert.True(!appointmentDateString.Equals(newFirstAppointmentDateString) ||
                !appointmentStartTime.Equals(newFirstppointmentStartTime));
        }

        [Fact]
        public void OtherSameDayAvailableAppointmentsAreDeletedWhenCreatingNewAppointment()
        {
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login");
            this.Login(AppConstants.AdministratorRoleName);
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/appointments");

            Actions actions = new Actions(this.browser);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Open following day availability modal
            var nextDayOnCalendarDisplay = this.browser.FindElement(this.CalendarDaySelector);
            actions.MoveToElement(nextDayOnCalendarDisplay, 5, 5).Click().Perform();
            wait.Until(b => b.FindElement(this.DailyAvailabilityModalSelector).Displayed);

            // Update daily availability and keep note of slot time
            var dailyModal = this.browser.FindElement(this.DailyAvailabilityModalSelector);
            var lastSlot = dailyModal.FindElements(this.DailyTimeSlotSelector).LastOrDefault();
            var lastSlotTime = lastSlot.Text.Trim(' ', '0').Split(':')[0];
            lastSlot.Click();

            // Submit availability
            var submitDailyAvailabilityBtn = dailyModal.FindElement(this.SubmitDailyAvailabilityBtnSelector);
            submitDailyAvailabilityBtn.Click();
            //this.WaitForAjax();

            wait.Until(b => !b.FindElement(this.SubmitDailyAvailabilityBtnSelector).Displayed);
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Open next day first appointment
            var firstAvailableAppointment = this.browser.FindElement(this.AppointmentSelector);
            firstAvailableAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert only new appointment exists for current day
            var firstAppointmentStartTime = this.browser.FindElement(
                this.AppointmentDetailsModalSelector)
                .FindElement(By.CssSelector(".start")).Text.Trim(' ', '0').Split(':')[0];
            Assert.Equal(lastSlotTime, firstAppointmentStartTime);
            Assert.Single(this.browser.FindElements(this.AppointmentSelector));
        }

        [Fact]
        public void AppointmentApprovalWorksCorrectly()
        {
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login");
            this.Login(AppConstants.AdministratorRoleName);
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/appointments");

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            Actions actions = new Actions(this.browser);

            // Get pending appointment and save its location
            var appointmentPendingApproval = this.browser.FindElements(this.AppointmentSelector).FirstOrDefault();

            var pendingApprovalElementLocation = appointmentPendingApproval.Location;

            // Open appointment details modal
            appointmentPendingApproval.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Approve appointment
            var approveAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                By.Id("approveAppointment"));
            approveAppointmentBtn.Click();

            wait.Until(b => !b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Click on same element
            actions.MoveByOffset(
                pendingApprovalElementLocation.X + 10, pendingApprovalElementLocation.Y + 10)
                .Click().Perform();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert appointment is approved
            var approvedColor = "color: rgb(146, 171, 149);";
            var statusSpanColor = this.browser.FindElement(
                By.CssSelector("#appointmentDetailsModal span.status"))
                .GetAttribute("style");

            Assert.Equal(approvedColor, statusSpanColor);
        }

        [Fact]
        public void PendingAppointmentCancellationWorksCorrectly()
        {
            var opts = new ChromeOptions();
            opts.AcceptInsecureCertificates = true;
            var chrome = new ChromeDriver(opts);

            chrome.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login");
            this.Login(AppConstants.AdministratorRoleName);
            chrome.Navigate().GoToUrl(this.server.RootUri + "/api/appointments");

            WebDriverWait wait = new WebDriverWait(chrome, TimeSpan.FromSeconds(10));
            Actions actions = new Actions(chrome);

            // Get pending appointment and save its location
            var appointmentPendingApproval = chrome.FindElements(this.AppointmentSelector).FirstOrDefault();
            var pendingApprovalElementLocation = appointmentPendingApproval.Location;

            // Open appointment details modal
            appointmentPendingApproval.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Cancel appointment
            var cancelAppointmentBtn = chrome.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                this.CancelAppointmentBtnSelector);
            cancelAppointmentBtn.Click();

            // Confirm cancellation
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = chrome
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            cancelConfirmBtn.Click();

            wait.Until(b => !b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Click on same element
            this.Click(chrome.FindElement(this.AppointmentSelector));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert appointment is now available (detals about user are not displayed => available appointment)
            Assert.False(chrome.FindElement(this.AppointmentDetailsModalSelector)
                .FindElement(By.CssSelector(".usernameGroup")).Displayed);
        }

        private void Click(IWebElement element)
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()", element);
        }

        private void Login(string role)
        {
            var username = this.configuration.GetSection($"{role}:Username").Value;
            var password = this.configuration.GetSection($"{role}:Password").Value;

            this.UsernameInputField.SendKeys(username);
            this.PasswordInputField.SendKeys(password);
            this.SubmitBtn.Click();
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
