namespace InSelfLove.Web.Tests
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;
    using CloudinaryDotNet.Actions;
    using InSelfLove.Data;
    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Appointments;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Videos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.UI;
    using Xunit;

    public class AppointmentPageTests : IClassFixture<SeleniumServerFactory<Startup>>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly SeleniumServerFactory<Startup> server;
        private readonly IWebDriver browser;
        private readonly IJavaScriptExecutor jsExecutor;

        public AppointmentPageTests(SeleniumServerFactory<Startup> server)
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
            //using (var scope = this.server.Server.Services.CreateScope())
            //{
            //    // Get repo
            //    var repo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<ApplicationUser>>();
            //    var repo1 = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<Appointment>>();

            //    var users = repo.All().ToListAsync().GetAwaiter().GetResult();
            //    var appts = repo1.All().ToListAsync().GetAwaiter().GetResult();
            //    ;
            //}

            // Log in
            this.Login(AppConstants.AdministratorRoleName);

            // Wait for calendar to load
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.CalendarDaySelector).Displayed);

            // Open daily availability modal
            // Click on last element with no children
            // First element may display appointment status(available/not)
            // differently depending on the time of day the test is run
            // If the day is in the future, we don't have that risk
            // So just get the last displayed day to be safe
            var emptyDay = this.browser
                .FindElements(this.CalendarDaySelector)
                .Where(e => e.FindElements(this.AppointmentSelector).Count == 0)
                .LastOrDefault();

            // Click our day in the calendar
            Actions actions = new Actions(this.browser);
            actions.MoveToElement(emptyDay, 5, 5).Click().Perform();
            wait.Until(b => b.FindElement(this.DailyAvailabilityModalSelector).Displayed);
            var dailyModal = this.browser.FindElement(this.DailyAvailabilityModalSelector);

            // Get first appointment slot
            var firstSlot = dailyModal.FindElement(this.DailyTimeSlotSelector);

            // Save its time
            var firstSlotTime = firstSlot.Text.Trim(' ', '0').Split(':')[0];

            // Select it and submit
            this.Click(firstSlot);
            var submitDailyAvailabilityBtn = dailyModal.FindElement(this.SubmitDailyAvailabilityBtnSelector);
            this.Click(submitDailyAvailabilityBtn);

            // Wait until browser refreshes
            wait.Until(b => !b.FindElement(this.DailyAvailabilityModalSelector).Displayed);

            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Click on our new appointment
            var appointment = this.browser.FindElements(this.AppointmentSelector).FirstOrDefault();
            appointment.Click();
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Confirm its start time matches what we selected from the daily availability modal
            var lastAppointmentStartTime = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(By.CssSelector(".start")).Text;
            Assert.Equal(firstSlotTime, lastAppointmentStartTime.Trim(' ', '0').Split(':')[0]);

            // Return db to empty state
            //this.ResetDb();
        }

        [Fact]
        public void AvailableAppointmentCancellationvoidsCorrectly()
        {
            // Create 1 available appt
            //this.CreateAppointments();

            // Log in & go to appts page
            this.Login(AppConstants.AdministratorRoleName);

            // Wait until calendar loads
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Open appointment details
            var appointment = this.browser.FindElement(this.AppointmentSelector);
            this.Click(appointment);

            // Extract date and start time
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            var appointmentDetailsModal = this.browser.FindElement(this.AppointmentDetailsModalSelector);

            // Cancel appointment
            var btnDelete = this.browser.FindElement(this.CancelAppointmentBtnSelector);
            this.Click(btnDelete);

            // Wait for cancellation confirm modal to display
            wait.Until(b => b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);

            // Get confirm cancellation btn & click
            var cancelConfirmBtn = this.browser
                .FindElement(this.CancelAppointmentConfirmModalSelector)
                .FindElement(By.CssSelector(".confirmCancelAppointment"));
            Actions actions = new Actions(this.browser);
            actions.MoveToElement(cancelConfirmBtn).Click().Perform();

            // Wait for calendar to reset
            wait.Until(b => !b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);
            wait.Until(b => b.FindElement(this.CalendarDaySelector).Displayed);

            // Assert we have no appointments in calendar
            var appts = this.browser.FindElements(this.AppointmentSelector);
            Assert.Empty(appts);

            // Reset db to default state
            //this.ResetDb();
        }

        // TODO: Check if async tests will work now
        [Fact]
        public void OtherSameDayAvailableAppointmentsDeletedWhenCreatingNewAppointment()
        {
            // Create 3 appts for 1 day ahead
            this.CreateAppointments(3, 1);

            // Log in & go to appts page
            this.Login(AppConstants.AdministratorRoleName);

            // Open day with appointments
            var dayWithAppointments = this.browser.FindElements(this.CalendarDaySelector)
                .Where(x => x.FindElements(this.AppointmentSelector).Count > 0).FirstOrDefault();

            Actions actions = new Actions(this.browser);
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));

            // Open daily availability modal
            actions.MoveToElement(dayWithAppointments, 40, 40).Click().Perform();
            wait.Until(b => b.FindElement(this.DailyAvailabilityModalSelector).Displayed);

            // Update daily availability and keep note of slot time
            var dailyModal = this.browser.FindElement(this.DailyAvailabilityModalSelector);
            var slot = dailyModal.FindElements(this.DailyTimeSlotSelector).LastOrDefault();
            var slotTime = slot.Text.Trim(' ', '0').Split(':')[0];
            slot.Click();

            // Submit availability
            var submitDailyAvailabilityBtn = dailyModal.FindElement(this.SubmitDailyAvailabilityBtnSelector);
            submitDailyAvailabilityBtn.Click();

            wait.Until(b => !b.FindElement(this.DailyAvailabilityModalSelector).Displayed);
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Assert we have just the one slot we entered
            var appts = this.browser.FindElements(this.AppointmentSelector);
            Assert.Single(this.browser.FindElements(this.AppointmentSelector));
            Assert.Equal(appts.First().Text.Trim(' ', '0').Split(':')[0], slotTime);

            // Reset db to default
            this.ResetDb();
        }

        [Fact]
        public void AppointmentApprovalWorksCorrectlvoid()
        {
            // Create 1 appt, 1 day ahead, awaiting approval
            this.CreateAppointments(1, 1, true);

            // Log in & go to appts page
            this.Login(AppConstants.AdministratorRoleName);
            
            // Get pending appointment and save its location
            var appointmentPendingApproval = this.browser.FindElements(this.AppointmentSelector).FirstOrDefault();
            var pendingApprovalElementLocation = appointmentPendingApproval.Location;

            // Open appointment details modal
            appointmentPendingApproval.Click();

            // Wait for appt details modal to display
            WebDriverWait wait = new WebDriverWait(this.browser, TimeSpan.FromSeconds(10));
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Approve appointment
            var approveAppointmentBtn = this.browser.FindElement(
                this.AppointmentDetailsModalSelector).FindElement(
                By.Id("approveAppointment"));
            approveAppointmentBtn.Click();

            // Wait for calendar to reset
            wait.Until(b => !b.FindElement(this.AppointmentDetailsModalSelector).Displayed);
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Click on same element
            Actions actions = new Actions(this.browser);
            actions.MoveByOffset(pendingApprovalElementLocation.X + 10, pendingApprovalElementLocation.Y + 10)
                                 .Click().Perform();

            // Wait for appt details modal to display
            wait.Until(b => b.FindElement(this.AppointmentDetailsModalSelector).Displayed);

            // Assert appointment is approved
            var approvedColor = "color: rgb(146, 171, 149);";
            var statusSpanColor = this.browser.FindElement(
                By.CssSelector("#appointmentDetailsModal span.status"))
                .GetAttribute("style");
            Assert.Equal(approvedColor, statusSpanColor);

            // Return db to default state
            this.ResetDb();
        }

        [Fact]
        public void PendingAppointmentCancellationWorksCorrectly()
        {
            // Create 1 appt 1 day ahead, awaiting approval
            this.CreateAppointments(1, 1, true);

            // Log in & go to appts page
            this.Login(AppConstants.AdministratorRoleName);

            // Get pending appointment and save its location
            var appointmentPendingApproval = this.browser.FindElements(this.AppointmentSelector).FirstOrDefault();
            var pendingApprovalElementLocation = appointmentPendingApproval.Location;

            // Open appointment details modal
            appointmentPendingApproval.Click();

            // Wait for appt details modal to display
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

            // Wait for browser to reset
            wait.Until(b => !b.FindElement(this.CancelAppointmentConfirmModalSelector).Displayed);
            wait.Until(b => b.FindElement(this.AppointmentSelector).Displayed);

            // Click on appt
            this.Click(this.browser.FindElement(this.AppointmentSelector));

            // Assert appointment is now available (detals about user are not displayed => available appointment)
            Assert.False(this.browser.FindElement(this.AppointmentDetailsModalSelector)
                .FindElement(By.CssSelector(".usernameGroup")).Displayed);

            // Return db to default state
            this.ResetDb();
        }

        private void CreateAppointments(int count = 1, int daysAhead = 1, bool awaiting = false, bool approved = false)
        {
            // Create scope
            using (var scope = this.server.Server.Services.CreateScope())
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
                    };

                    if (awaiting || approved)
                    {
                        // Get user & add to appt
                        var userRepo = scope.ServiceProvider.GetRequiredService<IDeletableEntityRepository<ApplicationUser>>();
                        var userName = this.configuration.GetSection($"{AppConstants.UserRoleName}:Username").Value;
                        appt.User = userRepo.All().FirstOrDefault(x => x.UserName == userName);
                        appt.IsApproved = approved ? true : false;
                    }

                    repo.AddAsync(appt).GetAwaiter().GetResult();
                }

                // Save changes
                repo.SaveChangesAsync().GetAwaiter().GetResult();
            }
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


        private void Click(IWebElement element)
        {
            this.jsExecutor.ExecuteScript("arguments[0].click()", element);
        }

        private void Login(string role)
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
                // Disposing of server after each test doesn't allow us to use the server to create scope
                // For retrieving services - disposed object error
                //this.server?.Dispose();
                this.browser?.Dispose();
            }
        }
    }
}
