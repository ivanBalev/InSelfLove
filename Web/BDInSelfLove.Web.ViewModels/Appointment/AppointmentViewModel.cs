namespace BDInSelfLove.Web.ViewModels.Appointment
{
    using System;

    using AutoMapper;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Appointment;

    public class AppointmentViewModel : IMapFrom<AppointmentServiceModel>, IHaveCustomMappings
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }

        public bool IsOwn { get; set; }

        public bool IsApproved { get; set; }

        public string UserUserName { get; set; }

        public string UserEmail { get; set; }

        public string UserId { get; set; }


        public string Description { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<AppointmentServiceModel, AppointmentViewModel>().ForMember(
                m => m.Start,
                opt => opt.MapFrom(x => x.UtcStart));
        }
    }
}
