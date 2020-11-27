namespace BDInSelfLove.Web.ViewModels.Appointment
{
    using System;
    using AutoMapper;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Appointment;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;

    public class AppointmentViewModel : IMapFrom<AppointmentServiceModel>
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }

        public bool IsOwn { get; set; }

        public bool IsApproved { get; set; }

        public string UserUserName { get; set; }

        public string UserEmail { get; set; }

        public string UserPhoneNumber { get; set; }

        public string Description { get; set; }
    }
}
