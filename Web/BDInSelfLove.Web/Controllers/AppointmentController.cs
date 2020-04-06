namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Calendar;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Services.Models.Appointment;
    using BDInSelfLove.Web.ViewModels.Calendar;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class AppointmentController : Controller
    {
        private readonly IAppointmentService appointmentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public AppointmentController(IAppointmentService appointmentService, UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.appointmentService = appointmentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }


        public ActionResult Index()
        {
            return this.View();
        }

        public async Task<JsonResult> GetAll()
        {
            var appointmentViewModel = await this.appointmentService.GetAll()
                .To<AppointmentViewModel>()
                .ToListAsync();

            return new JsonResult(appointmentViewModel);
        }

        [HttpPost]
        public async Task<JsonResult> Save(AppointmentInputModel inputModel)
        {
            var request = this.HttpContext;

            var status = false;
            var serviceModel = AutoMapperConfig.MapperInstance.Map<AppointmentServiceModel>(inputModel);

            // Validate
            var dbCheck = await this.appointmentService.GetAllByDate(serviceModel.Start).Where(a => a.Start.Hour == serviceModel.Start.Hour).FirstOrDefaultAsync();

            if (dbCheck != null)
            {
                return null;
            }

            if (inputModel.Id > 0)
            {
                await this.appointmentService.Edit(serviceModel);
            }
            else
            {
                var user = await this.userManager.GetUserAsync(this.User);
                var userId = user.Id;
                serviceModel.UserId = userId;

                await this.appointmentService.Create(serviceModel);

                await this.emailSender.SendEmailAsync(
                                            user.Email,
                                            user.UserName,
                                            GlobalConstants.SystemEmail,
                                            GlobalConstants.SystemName + " " + GlobalConstants.AppointmentEmailSubject,
                                            serviceModel.Start.ToString("dd MMMM HH:mm") + Environment.NewLine + serviceModel.Description);
            }

            status = true;

            return new JsonResult(new { status });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var status = false;

            // TODO: the check below is temporary. Needs to be reworked
            var creatorId = (await this.appointmentService.GetById(id)).UserId;
            var currentUserId = (await this.userManager.GetUserAsync(this.User)).Id;

            if (creatorId != currentUserId)
            {
                return new JsonResult(status);
            }


            var deleteResult = await this.appointmentService.Delete(id);

            status = true;
            return new JsonResult(new { status });
        }

        [HttpGet]
        public async Task<JsonResult> GetAppointmentsByDate(string date)
        {
            var formattedDate = string.Join(" ", date.Split(" ").Skip(1).Take(3));
            var parsedDate = DateTime.ParseExact(formattedDate, "MMM dd yyyy", CultureInfo.InvariantCulture);

            var currentDayAppointments = await this.appointmentService.GetAllByDate(parsedDate)
                .To<AppointmentViewModel>()
                .ToListAsync();

            var availableHours = Enumerable.Range(9, 10);

            var returnList = new List<AppointmentViewModel>();

            foreach (var item in availableHours)
            {
                if (currentDayAppointments.Any(a => a.Start.Hour == item))
                {
                    continue;
                }

                returnList.Add(new AppointmentViewModel { Start = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, item, 0, 0) });
            }

            return new JsonResult(returnList);
        }
    }
}
