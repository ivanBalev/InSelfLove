using AutoMapper;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Appointment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Appointment
{
    public class AppointmentInputModel : IMapTo<AppointmentServiceModel>, IHaveCustomMappings
    {
        [Required]
        public string Start { get; set; }

        [Required]
        [MinLength(30)]
        public string Description { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<AppointmentInputModel, AppointmentServiceModel>().ForMember(
                m => m.Start,
                opt => opt.MapFrom(x => DateTime.ParseExact(x.Start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
        }
    }
}
