namespace InSelfLove.Web.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Appointments;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using SeleniumExtras.WaitHelpers;
    using Xunit;

    public class AppointmentPageUserTests : IClassFixture<SeleniumServerFactory<TestStartup>>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<TestStartup> server;
        private readonly IJavaScriptExecutor jsExecutor;
        private readonly IWebDriver browser;

        public AppointmentPageUserTests(SeleniumServerFactory<TestStartup> server)
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

        private By AppointmentDetailsModalSelector => By.Id("appointmentDetailsModal");

        private By AppointmentSelector => By.CssSelector(".fc-event");

        private By CancelAppointmentBtnSelector => By.Id("btnDelete");

        private By CancelAppointmentConfirmModalSelector => By.Id("cancelAppointmentConfirm");

        private By BookAppointmentModalSelector => By.Id("bookAppointmentModal");

        [Fact]
        public void GuestCannotBookAppointments()
        {
            // Create an appt and go to appts page
            this.CreateAppointments();
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/Appointments");

            // Click on available appt
            var availableAppointment = this.browser.FindElement(this.AppointmentSelector);
            availableAppointment.Click();

            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(By.Id("loginModal")).Displayed);

            // Assert user is prompted to log in
            Assert.True(this.browser.FindElement(By.Id("loginModal")).Displayed);
        }

        [Fact]
        public void UserSeesOthersAppointmentsCorrectly()
        {
            // Create an appt and go to appts page
            this.CreateAppointments(unavailable: true);
            this.browser.Navigate().GoToUrl(this.server.RootUri + "/api/Appointments");

            // Ensure appt is styled as unavailable
            var appointment = this.browser.FindElement(this.AppointmentSelector);
            Assert.Contains("gray", appointment.GetAttribute("class"));

            // Ensure no action takes place when unavailable appt is clicked
            appointment.Click();
            Assert.False(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);
        }

        [Fact]
        public void OtherSameDayAppointmentsBecomeUnavailableWhenUserBooks()
        {
            this.CreateAppointments(3);
            this.Login(AppConstants.UserRoleName);

            // Book first appt
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.BookFirstAvailableAppointment(wait);

            // Get the rest of the appts
            var otherAppointments = this.browser.FindElements(this.AppointmentSelector).Skip(1).ToList();

            // Assert they're all listed as unavailable (user can only book 1 appt per day by design)
            foreach (var appt in otherAppointments)
            {
                // Ensure styling is correct
                Assert.Contains("gray", appt.GetAttribute("class"));

                // Ensure nothing happens when user clicks on appt
                appt.Click();
                Assert.False(this.browser.FindElement(this.BookAppointmentModalSelector).Displayed);
            }
        }

        [Fact]
        public void AppointmentBookingWorksCorrectly()
        {
            this.CreateAppointments();
            var username = this.Login(AppConstants.UserRoleName);

            // Book first appt
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.BookFirstAvailableAppointment(wait);

            // Click on first appt
            var myAppointment = this.browser.FindElement(this.AppointmentSelector);
            myAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert it's now awaiting approval for current user
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);
            var usernameOnDetailsModal = appointmentDetailsModal.FindElement(
                By.CssSelector(".usernameGroup .username")).Text;
            Assert.Equal(username, usernameOnDetailsModal);
        }

        [Fact]
        public void AppointmentOnsiteBookingWorksCorrectly()
        {
            // Create appt that's available for onsite booking
            this.CreateAppointments(onSite: true);
            this.Login(AppConstants.UserRoleName);

            // Book appt
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            this.BookFirstAvailableAppointment(wait, true);

            // Open details modal for appt
            var myAppointment = this.browser.FindElement(this.AppointmentSelector);
            myAppointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Ensure user is informed the appt will be onsite
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);
            var onSiteMsg = appointmentDetailsModal.FindElement(By.Id("onSiteDetailsMsg"));
            Assert.True(onSiteMsg.Displayed);
        }

        [Fact]
        public void PendingAppointmentCancellationWorksCorrectly()
        {
            this.CreateAppointments(1, 1, true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment and save its location
            var appointmentPendingApproval = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            appointmentPendingApproval.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
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
        }

        [Fact]
        public void ApprovedAppointmentCancellationWorksCorrectly()
        {
            this.CreateAppointments(1, 1, false, true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment
            var approvedAppointment = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            approvedAppointment.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
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
        }

        [Fact]
        public void PendingAppointmentCannotBePaid()
        {
            this.CreateAppointments(1, 1, true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment
            var appointmentPendingApproval = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            appointmentPendingApproval.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // With only a single pending appointment, SSR doesn't provide the payment btn at all
            Assert.Throws<NoSuchElementException>(() => this.browser.FindElement(By.Id("payBtn")));
        }

        [Fact]
        public void PendingAndApprovedAppointmentsPayBtnStateIsCorrect()
        {
            // Create approved & pending appts
            this.CreateAppointments(1, 1, awaiting: true);
            this.CreateAppointments(1, 2, approved: true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment
            var appointmentPendingApproval = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            appointmentPendingApproval.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Payment btn should be there since we have another appt on the page which is approved
            // but it shouldn't be visible
            Assert.False(this.browser.FindElement(By.Id("payBtn")).Displayed);

            // Get approved appt
            var approvedAppointment = this.browser.FindElements(this.AppointmentSelector).LastOrDefault();

            // Open appointment details modal
            this.Click(approvedAppointment);
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Payment btn should be visible
            Assert.True(this.browser.FindElement(By.Id("payBtn")).Displayed);
        }

        [Fact]
        public void PaidAppointmentDisplayedInDetailsModal()
        {
            this.CreateAppointments(1, 1, approved: true, paid: true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment
            var appointment = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            appointment.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Asser payment status is displayed
            var paidSpan = this.browser.FindElement(this.AppointmentDetailsModalSelector)
                .FindElement(By.ClassName("paid"));
            Assert.True(paidSpan.Text != string.Empty);
        }

        [Fact]
        public void PaymentWorksWithValidCardNumber()
        {
            // Provide Stripe CLI with endpoint for submitting events
            var command = $"stripe listen --forward-to {this.server.RootUri}/stripe/confirmpay";
            ProcessStartInfo stripeEvent = new ProcessStartInfo();
            stripeEvent.FileName = "cmd.exe";
            stripeEvent.WindowStyle = ProcessWindowStyle.Normal;
            stripeEvent.Arguments = "/k " + command;
            Process.Start(stripeEvent);

            this.CreateAppointments(approved: true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment
            var appointment = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            appointment.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Open payment modal
            this.browser.FindElement(By.Id("payBtn")).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#payment-element iframe")));

            // Switch browser context to Stripe iFrame. Otherwise we're not able to select its nested elements
            var stripeIFrame = this.browser.FindElement(By.CssSelector("#payment-element iframe"));
            this.browser.SwitchTo().Frame(stripeIFrame);

            // Populate fields
            var cardNumberField = this.browser.FindElement(By.CssSelector("[name=number]"));
            cardNumberField.SendKeys("4242 4242 4242 4242");

            var expiryField = this.browser.FindElement(By.CssSelector("[name=expiry]"));
            expiryField.SendKeys("04/24");

            var cvcField = this.browser.FindElement(By.CssSelector("[name=cvc]"));
            cvcField.SendKeys("424");

            // Submit data
            this.browser.SwitchTo().DefaultContent();
            this.browser.FindElement(By.CssSelector("#payment-form #submit")).Click();

            // Assert payment is confirmed on the front end
            this.WaitForBrowserToRefresh(wait, By.Id("payment-form-modal"), By.Id("success-modal"));
            Assert.True(this.browser.FindElement(By.Id("success-modal")).Displayed);

            using (var scope = this.server.Services.CreateScope())
            {
                // Get repos
                var paymentRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Payment>>();
                var appointmentRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Appointment>>();
                var userRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<ApplicationUser>>();

                var userName = this.configuration.GetSection($"{AppConstants.UserRoleName}:Username").Value;

                // Get user, appt & payment
                var user = userRepo.All().FirstOrDefault(x => x.UserName == userName);
                var apptmnt = appointmentRepo.All().FirstOrDefault();
                var payment = paymentRepo.All().FirstOrDefault();

                // Assert payment is registered in db
                Assert.Equal(payment.ApplicationUserId, user.Id);
                Assert.Equal(payment.AppointmentId, apptmnt.Id);
            }
        }

        [Fact]
        public void PaymentDoesntWorkWithInvalidCardNumber()
        {
            // Provide stripe CLI with endpoint for submitting events
            var command = $"stripe listen --forward-to {this.server.RootUri}/stripe/confirmpay";
            ProcessStartInfo stripeEvent = new ProcessStartInfo();
            stripeEvent.FileName = "cmd.exe";
            stripeEvent.WindowStyle = ProcessWindowStyle.Normal;
            stripeEvent.Arguments = "/k " + command;
            Process.Start(stripeEvent);

            this.CreateAppointments(approved: true);
            this.Login(AppConstants.UserRoleName);

            // Get pending appointment
            var appointment = this.browser.FindElement(this.AppointmentSelector);

            // Open appointment details modal
            appointment.Click();
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Open payment modal
            this.browser.FindElement(By.Id("payBtn")).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#payment-element iframe")));

            // Switch browser context to Stripe iFrame. Otherwise we're not able to select its nested elements
            var stripeIFrame = this.browser.FindElement(By.CssSelector("#payment-element iframe"));
            this.browser.SwitchTo().Frame(stripeIFrame);

            // Populate fields
            var cardNumberField = this.browser.FindElement(By.CssSelector("[name=number]"));
            cardNumberField.SendKeys("4100 0000 0000 0019");

            var expiryField = this.browser.FindElement(By.CssSelector("[name=expiry]"));
            expiryField.SendKeys("04/24");

            var cvcField = this.browser.FindElement(By.CssSelector("[name=cvc]"));
            cvcField.SendKeys("424");

            // Submit data
            this.browser.SwitchTo().DefaultContent();
            this.browser.FindElement(By.CssSelector("#payment-form #submit")).Click();
            this.WaitForBrowserToRefresh(wait, By.Id("payment-form-modal"), By.Id("fail-modal"));

            // Assert payment failure is registered on front end
            Assert.True(this.browser.FindElement(By.Id("fail-modal")).Displayed);

            using (var scope = this.server.Services.CreateScope())
            {
                // Get repo
                var paymentRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Payment>>();
                var payment = paymentRepo.All().ToList();

                // Assert payment is not registered in db as well
                Assert.Empty(payment);
            }
        }

        private void CreateAppointments(
            int count = 1,
            int daysAhead = 1,
            bool awaiting = false,
            bool approved = false,
            bool unavailable = false,
            bool onSite = false,
            bool paid = false)
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
                        IsPaid = paid,
                    };

                    if (awaiting || approved || unavailable)
                    {
                        var userRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<ApplicationUser>>();

                        if (unavailable)
                        {
                            // Attribute the appt with a user
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
            // Click on 1st available appt
            var availableAppointment = this.browser.FindElement(this.AppointmentSelector);
            availableAppointment.Click();
            wait.Until(b => b.FindElement(this.BookAppointmentModalSelector).Displayed);
            var bookAppointmentModal = this.browser.FindElement(this.BookAppointmentModalSelector);

            // Populate issue description
            var issueDescriptionTextbox = bookAppointmentModal.FindElement(By.Id("patientIssueDescription"));
            issueDescriptionTextbox.SendKeys(new string('a', 31));

            if (onSite)
            {
                // Click on onsite checkbox
                var onsiteCheckbox = this.browser.FindElement(
               this.BookAppointmentModalSelector).FindElement(
               By.CssSelector("#onSiteBookToggle .toggle-checkbox"));
                this.Click(onsiteCheckbox);
            }

            // Click the submit btn
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
                this.ResetDb();
                this.browser.Manage().Cookies.DeleteAllCookies();
            }
        }
    }
}
