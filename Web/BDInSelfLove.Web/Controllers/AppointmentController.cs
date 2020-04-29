namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Calendar;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Services.Models.Appointment;
    using BDInSelfLove.Web.Areas.Administration;
    using BDInSelfLove.Web.Infrastructure.ModelBinders;
    using BDInSelfLove.Web.InputModels.Appointment;
    using BDInSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private const string AppointmentEmailSubject = "Appointment";
        private const string AppointmentDeleteEmailBody = "The client opted to cancel their appointment";

        private readonly IAppointmentService appointmentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public AppointmentController(
            IAppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.appointmentService = appointmentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<ICollection<AppointmentViewModel>>> GetAll()
        {
            string userId = this.User.IsInRole(GlobalConstants.AdministratorRoleName) ?
                null : this.userManager.GetUserId(this.User);

            var appointmentViewModel = await this.appointmentService.GetAll(userId)
                .To<AppointmentViewModel>()
                .ToListAsync();

            return appointmentViewModel;
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save([FromForm]AppointmentInputModel inputModel)
        {
            var serviceModel = AutoMapperConfig.MapperInstance.Map<AppointmentServiceModel>(inputModel);

            // Validation
            var appointmentSlotIsOccupied = await this.appointmentService
                                                      .GetAllByDate(serviceModel.Start)
                                                      .Where(a => a.Start.Hour == serviceModel.Start.Hour)
                                                      .FirstOrDefaultAsync();
            if (appointmentSlotIsOccupied != null)
            {
                return this.BadRequest(new { status = false });
            }

            var user = await this.userManager.GetUserAsync(this.User);
            serviceModel.UserId = user.Id;

            await this.appointmentService.Create(serviceModel);
            await this.emailSender.SendEmailAsync(
                                        user.Email,
                                        user.UserName,
                                        GlobalConstants.SystemEmail,
                                        GlobalConstants.SystemName + " " + AppointmentEmailSubject,
                                        serviceModel.Start.ToString("dd MMMM HH:mm") + "<br>" + serviceModel.Description);

            return this.Ok(new { status = true });
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromForm]int id)
        {
            var creatorId = (await this.appointmentService.GetById(id)).UserId;
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (creatorId != currentUser.Id)
            {
                return this.BadRequest(new { status = false });
            }

            await this.appointmentService.Delete(id);

            await this.emailSender.SendEmailAsync(
                                        currentUser.Email,
                                        currentUser.UserName,
                                        GlobalConstants.SystemEmail,
                                        GlobalConstants.SystemName + " " + AppointmentEmailSubject,
                                        AppointmentDeleteEmailBody);

            return this.Ok(new { status = true });
        }

        [HttpGet]
        [Route("GetAppointmentsByDate")]
        public async Task<ActionResult<ICollection<AppointmentViewModel>>> GetAppointmentsByDate([ModelBinder(typeof(AppointmentDateBinder))]DateTime date)
        {
            if (DateTime.Compare(date, DateTime.UtcNow) <= 0)
            {
                return this.BadRequest(new { status = false });
            }

            var currentDayAppointments = await this.appointmentService.GetAllByDate(date)
                .To<AppointmentViewModel>()
                .ToListAsync();

            var availableHours = Enumerable.Range(GlobalAdminValues.WorkDayStart, GlobalAdminValues.WorkDayEnd - GlobalAdminValues.WorkDayStart);

            var returnList = new List<AppointmentViewModel>();

            foreach (var hour in availableHours)
            {
                if (currentDayAppointments.Any(a => a.Start.Hour == hour))
                {
                    continue;
                }

                returnList.Add(new AppointmentViewModel { Start = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0) });
            }

            return returnList;
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [Route("SetWorkingHours")]
        public ActionResult SetWorkingHours([FromForm]string startHour, [FromForm]string endHour)
        {
            GlobalAdminValues.WorkDayStart = int.Parse(startHour.Split(':')[0]);
            GlobalAdminValues.WorkDayEnd = int.Parse(endHour.Split(':')[0]);

            return this.Ok(new { status = true });
        }
    }
}
