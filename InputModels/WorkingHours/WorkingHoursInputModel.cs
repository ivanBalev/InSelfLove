using AutoMapper;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Appointment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BDInSelfLove.Web.InputModels.WorkingHours
{
    public class WorkingHoursInputModel : IMapTo<AppointmentServiceModel>, IHaveCustomMappings
    {
        public WorkingHoursInputModel()
        {
            this.TimeSlots = new List<string>();
        }

        [Required]
        public IEnumerable<string> TimeSlots { get; set; }


        public void CreateMappings(IProfileExpression configuration)
        {
            var cleanSlots = new string[this.TimeSlots.Count()];

            var index = 0;

            foreach (var slot in this.TimeSlots)
            {
                cleanSlots[index++] = string.Join(' ', slot.Split(' ').Skip(1).Take(4));
            }


            //configuration.CreateMap<WorkingHoursInputModel, AppointmentServiceModel>().ForMember(
            //    m => m.Start,
            //    opt => opt.MapFrom(x => DateTime.ParseExact(onlyNecessaryInfo, "MMM dd yyyy HH:mm  Wed Nov 04 2020 17:00", CultureInfo.InvariantCulture)));
        }
    }
}
