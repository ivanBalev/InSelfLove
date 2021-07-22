using AutoMapper;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Appointment;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Appointment
{
    public class AppointmentInputModel : IMapTo<AppointmentServiceModel>
    {
        [Required]
        public string Start { get; set; }

        [Required]
        [MinLength(30)]
        public string Description { get; set; }
    }
}
