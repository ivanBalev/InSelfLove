namespace BDInSelfLove.Web.Tests
{
    using System;
    using System.Linq;
    using System.Threading;

    using BDInSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class AppointmentPageTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private const string ApprovedText = "✓";
        private const string AwaitingApprovalText = "\u27F3";

        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;

        public AppointmentPageTests(SeleniumServerFactory<TestStartup> server)
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

        private By CalendarDaySelector => By.CssSelector(".fc-content-col");

        private By AppointmentSelector => By.CssSelector(".fc-event");

        private By DailyAvailabilityModalSelector => By.Id("dailyAvailabilityModal");

        private By DailyTimeSlotSelector => By.CssSelector(".dailyTimeSlot");

        private By SubmitDailyAvailabilityBtnSelector => By.Id("submitDailyAvailability");

        private By CancelAppointmentBtnSelector => By.Id("btnDelete");

        private By CancelAppointmentConfirmModalSelector => By.Id("cancelAppointmentConfirm");

        // Admin tests
        [Fact]
        public void AvailableAppointmentCancellationWorksCorrectly()
        {
            this.Login(AppConstants.AdministratorRoleName);
            this.WaitForAjax();

            // Open first appointment details
            var firstAppointment = this.browser.FindElement(this.AppointmentSelector);
            firstAppointment.Click();

            // Extract date and start time
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);

            var appointmentDateString = appointmentDetailsModal.FindElement(By.CssSelector(".date")).Text;
            var appointmentStartTime = appointmentDetailsModal.FindElement(By.CssSelector(".start")).Text;

            // Delete appointment
            var btnDelete = this.browser.FindElement(this.CancelAppointmentBtnSelector);
            btnDelete.Click();

            // Confirm deletion
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            cancelConfirmBtn.Click();
            this.WaitForAjax();

            // Open new first appointment details
            var newFirstAppointment = this.browser.FindElement(this.AppointmentSelector);
            newFirstAppointment.Click();
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
        public void AppointmentCreationWorksCorrectly()
        {
            this.Login(AppConstants.AdministratorRoleName);
            this.WaitForAjax();

            Actions actions = new Actions(this.browser);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Open daily availability modal
            var lastDayOnCalendarDisplay = this.browser.FindElements(this.CalendarDaySelector).LastOrDefault();
            actions.MoveToElement(lastDayOnCalendarDisplay, 30, 30).Click().Perform();
            wait.Until(b => b.FindElement(this.DailyAvailabilityModalSelector).Displayed);
            var dailyModal = this.browser.FindElement(this.DailyAvailabilityModalSelector);

            // Select a slot and submit
            var firstSlot = dailyModal.FindElement(this.DailyTimeSlotSelector);
            var firstSlotTime = firstSlot.Text.Trim(' ', '0');
            firstSlot.Click();
            var submitDailyAvailabilityBtn = dailyModal.FindElement(this.SubmitDailyAvailabilityBtnSelector);
            submitDailyAvailabilityBtn.Click();
            this.WaitForAjax();

            // Confirm appointment has been created (other same-day appointments have been deleted by design)
            var lastAppointment = this.browser.FindElements(this.AppointmentSelector).LastOrDefault();
            lastAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            var lastAppointmentStartTime = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(By.CssSelector(".start")).Text;
            Assert.Equal(firstSlotTime, lastAppointmentStartTime.Trim(' ', '0'));
        }

        [Fact]
        public void OtherSameDayAvailableAppointmentsAreDeletedWhenCreatingNewAppointment()
        {
            this.Login(AppConstants.AdministratorRoleName);
            this.WaitForAjax();

            Actions actions = new Actions(this.browser);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Open following day availability modal
            var nextDayOnCalendarDisplay = this.browser.FindElements(this.CalendarDaySelector)[1];
            actions.MoveToElement(nextDayOnCalendarDisplay, 30, 300).Click().Perform();
            wait.Until(b => b.FindElement(this.DailyAvailabilityModalSelector).Displayed);

            // Update daily availability and keep note of slot time
            var dailyModal = this.browser.FindElement(this.DailyAvailabilityModalSelector);
            var lastSlot = dailyModal.FindElements(this.DailyTimeSlotSelector).LastOrDefault();
            var lastSlotTime = lastSlot.Text.Trim(' ', '0');
            lastSlot.Click();

            // Submit availability
            var submitDailyAvailabilityBtn = dailyModal.FindElement(this.SubmitDailyAvailabilityBtnSelector);
            submitDailyAvailabilityBtn.Click();
            this.WaitForAjax();

            // Open next day first appointment
            var firstAvailableAppointment = this.browser.FindElements(
                this.CalendarDaySelector)[1].FindElements(
                this.AppointmentSelector)
                .Where(a => !a.Text.Equals(ApprovedText) && !a.Text.Equals(AwaitingApprovalText))
                .FirstOrDefault();
            firstAvailableAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert only new appointment exists for current day
            var firstAppointmentStartTime = this.browser.FindElement(
                this.AppointmentDetailsModalSelector)
                .FindElement(By.CssSelector(".start")).Text.Trim(' ', '0');
            Assert.Equal(lastSlotTime, firstAppointmentStartTime);
        }

        [Fact]
        public void AppointmentApprovalWorksCorrectly()
        {
            this.Login(AppConstants.AdministratorRoleName);
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

            // Approve appointment
            var approveAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                By.Id("approveAppointment"));
            approveAppointmentBtn.Click();
            this.WaitForAjax();

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
            this.Login(AppConstants.AdministratorRoleName);
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
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert appointment is now available (detals about user are not displayed => available appointment)
            Assert.False(this.browser.FindElement(this.AppointmentDetailsModalSelector)
                .FindElement(By.CssSelector(".usernameGroup")).Displayed);
        }

        private void Login(string role)
        {
            var username = this.configuration.GetSection($"{role}:Username").Value;
            var password = this.configuration.GetSection($"{role}:Password").Value;

            this.UsernameInputField.SendKeys(username);
            this.PasswordInputField.SendKeys(password);
            this.SubmitBtn.Click();
            this.WaitForAjax();
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
