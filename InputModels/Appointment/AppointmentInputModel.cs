using AutoMapper;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Appointment;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BDInSelfLove.Web.InputModels.Appointment
{
    public class AppointmentInputModel : IMapTo<AppointmentServiceModel>, IHaveCustomMappings
    {
        [Required]
        public string Start { get; set; }

        [Required]
        [MinLength(30)]
        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<AppointmentInputModel, AppointmentServiceModel>().ForMember(
                m => m.Start,
                opt => opt.MapFrom(x => DateTime.ParseExact(x.Start, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)));
        }
    }
}
