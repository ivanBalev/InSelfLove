namespace InSelfLove.Web.Tests
{
    using System;
    using System.Linq;
    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Appointments;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class AppointmentPageUserTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IWebDriver browser;
        private readonly IJavaScriptExecutor jsExecutor;

        public AppointmentPageUserTests(SeleniumServerFactory<TestStartup> server)
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

        private By AppointmentSelector => By.CssSelector(".fc-event");

        private By CancelAppointmentBtnSelector => By.Id("btnDelete");

        private By CancelAppointmentConfirmModalSelector => By.Id("cancelAppointmentConfirm");

        private By BookAppointmentModalSelector => By.Id("bookAppointmentModal");

        [Fact]
        public void GuestCannotBookAppointments()
        {
            this.CreateAppointments();

            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/Appointments");

            var availableAppointment = this.browser.FindElement(this.AppointmentSelector);
            availableAppointment.Click();

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(By.Id("loginModal")).Displayed);
            Assert.True(this.browser.FindElement(By.Id("loginModal")).Displayed);

            this.ResetDb();
        }

        [Fact]
        public void UserSeesOthersAppointmentsCorrectly()
        {
            this.CreateAppointments(unavailable: true);

            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/Appointments");

            var appointment = this.browser.FindElement(this.AppointmentSelector);
            Assert.Contains("gray", appointment.GetAttribute("class"));

            appointment.Click();
            Assert.False(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);

            this.ResetDb();
        }

        [Fact]
        public void OtherSameDayAppointmentsBecomeUnavailableWhenUserBooks()
        {
            this.CreateAppointments(3);
            this.Login(AppConstants.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.BookFirstAvailableAppointment(wait);

            var otherAppointments = this.browser.FindElements(this.AppointmentSelector).Skip(1).ToList();

            foreach (var appt in otherAppointments)
            {
                Assert.Contains("gray", appt.GetAttribute("class"));

                appt.Click();
                Assert.False(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);
            }

            this.ResetDb();
        }

        [Fact]
        public void AppointmentBookingWorksCorrectly()
        {
            this.CreateAppointments();
            var username = this.Login(AppConstants.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.BookFirstAvailableAppointment(wait);

            var myAppointment = this.browser.FindElement(this.AppointmentSelector);
            myAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Check if it's now awaiting approval for current user
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);
            var usernameOnDetailsModal = appointmentDetailsModal.FindElement(
                By.CssSelector(".usernameGroup .username")).Text;

            Assert.Equal(username, usernameOnDetailsModal);

            this.ResetDb();
        }

        [Fact]
        public void AppointmentOnsiteBookingWorksCorrectly()
        {
            this.CreateAppointments(onSite: true);
            this.Login(AppConstants.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.BookFirstAvailableAppointment(wait, true);

            var myAppointment = this.browser.FindElement(this.AppointmentSelector);
            myAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);
            var onSiteMsg = appointmentDetailsModal.FindElement(By.Id("onSiteDetailsMsg"));

            Assert.True(onSiteMsg.Displayed);

            this.ResetDb();
        }

        [Fact]
        public void PendingAppointmentCancellation()
        {
            this.CreateAppointments(1, 1, true);
            this.Login(AppConstants.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Get pending appointment and save its location
            var appointmentPendingApproval = this.browser.FindElements(this.AppointmentSelector).FirstOrDefault();

            // Open appointment details modal
            appointmentPendingApproval.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Cancel appointment
            var cancelAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                this.CancelAppointmentBtnSelector);
            cancelAppointmentBtn.Click();

            // Confirm cancellation
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            cancelConfirmBtn.Click();
            this.WaitForBrowserToRefresh(wait, this.CancelAppointmentConfirmModalSelector, this.AppointmentSelector);

            // Click on same element
            var availableAppt = this.browser.FindElement(this.AppointmentSelector);
            availableAppt.Click();
            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);

            // Assert book appointment modal is now displayed
            Assert.True(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);

            this.ResetDb();
        }

        [Fact]
        public void ApprovedAppointmentCancellation()
        {
            this.CreateAppointments(1, 1, false, true);
            this.Login(AppConstants.UserRoleName);

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Get pending appointment and save its location
            var approvedAppointment = this.browser.FindElements(this.AppointmentSelector).FirstOrDefault();

            // Open appointment details modal
            approvedAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Cancel appointment
            var cancelAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                this.CancelAppointmentBtnSelector);
            cancelAppointmentBtn.Click();

            // Confirm cancellation
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));

            cancelConfirmBtn.Click();
            this.WaitForBrowserToRefresh(wait, this.CancelAppointmentConfirmModalSelector, this.AppointmentSelector);

            // Click on same element
            var availableAppt = this.browser.FindElement(this.AppointmentSelector);
            availableAppt.Click();

            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);

            // Assert book appointment modal is now displayed
            Assert.True(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);

            this.ResetDb();
        }

        private void CreateAppointments(
            int count = 1,
            int daysAhead = 1,
            bool awaiting = false,
            bool approved = false,
            bool unavailable = false,
            bool onSite = false)
        {
            // Create scope
            using (var scope = this.server.Services.CreateScope())
            {
                // Get repo
                var repo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Appointment>>();

                // Create appts
                for (int i = 0; i < count; i++)
                {
                    var appt = new Appointment
                    {
                        // Create appts for selected days ahead and subsequent hours
                        UtcStart = DateTime.UtcNow.Date
                                           .AddDays(daysAhead)
                                           .AddHours(AppointmentService.DefaultWorkdayStart + i),
                        CanBeOnSite = onSite,
                    };

                    if (awaiting || approved || unavailable)
                    {
                        var userRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<ApplicationUser>>();

                        if (unavailable)
                        {
                            var userName = this.configuration.GetSection($"{AppConstants.AdministratorRoleName}:Username").Value;
                            appt.User = userRepo.All().FirstOrDefault(x => x.UserName == userName);
                        }
                        else
                        {
                            // Get user & add to appt
                            var userName = this.configuration.GetSection($"{AppConstants.UserRoleName}:Username").Value;
                            appt.User = userRepo.All().FirstOrDefault(x => x.UserName == userName);
                            appt.IsApproved = approved ? true : false;
                        }
                    }

                    repo.AddAsync(appt).GetAwaiter().GetResult();
                }

                // Save changes
                repo.SaveChangesAsync().GetAwaiter().GetResult();
            }
        }

        private void BookFirstAvailableAppointment(WebDriverWait wait, bool onSite = false)
        {
            var availableAppointment = this.browser.FindElement(this.AppointmentSelector);
            availableAppointment.Click();

            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);
            var bookAppointmentModal = this.browser.FindElement(this.BookAppointmentModalSelector);

            // Populate issue description
            var issueDescriptionTextbox = bookAppointmentModal.FindElement(By.Id("patientIssueDescription"));
            issueDescriptionTextbox.SendKeys(new string('a', 31));

            if (onSite)
            {
                var onsiteCheckbox = this.browser.FindElement(
               this.BookAppointmentModalSelector).FindElement(
               By.CssSelector("#onSiteBookToggle .toggle-checkbox"));
                this.Click(onsiteCheckbox);
            }

            // Click book appointment btn
            var sendAppointmentBtn = bookAppointmentModal.FindElement(By.Id("sendAppointment"));
            sendAppointmentBtn.Click();

            this.WaitForBrowserToRefresh(wait, this.BookAppointmentModalSelector, this.AppointmentSelector);
        }


        private void ResetDb()
        {
            // Empty appointments collection in server
            using (var scope = this.server.Server.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Appointment>>();
                var appts = repo.All().Where(x => !x.IsDeleted).ToList();

                foreach (var appt in appts)
                {
                    repo.Delete(appt);
                }

                repo.SaveChangesAsync().GetAwaiter().GetResult();
            }
        }

        private void WaitForBrowserToRefresh(WebDriverWait wait, By elementToBeHiddenSelector, By elementToBeDisplayedSelector)
        {
            // Stale element error if we don't use the strategy below
            try
            {
                // Wait for calendar to reset
                wait.Until(b => !b.FindElement(elementToBeHiddenSelector).Displayed);
                wait.Until(b => b.FindElement(elementToBeDisplayedSelector).Displayed);
            }
            catch (StaleElementReferenceException ex)
            {
                wait.Until(b => !b.FindElement(elementToBeHiddenSelector).Displayed);
                wait.Until(b => b.FindElement(elementToBeDisplayedSelector).Displayed);
            }
        }

        private void Click(IWebElement element)
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()", element);
        }

        private string Login(string role)
        {
            // Go to login page
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/Identity/Account/Login?ReturnUrl=/api/Appointments");

            // Get username & pass from config
            var username = this.configuration.GetSection($"{role}:Username").Value;
            var password = this.configuration.GetSection($"{role}:Password").Value;

            // Enter username & pass in respective fields & submit
            this.UsernameInputField.SendKeys(username);
            this.PasswordInputField.SendKeys(password);
            this.SubmitBtn.Click();

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
                //this.server?.Dispose();
                this.browser?.Dispose();
            }
        }
    }
}
