namespace BDInSelfLove.Services.Models.Appointment
{
    using System;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.User;

    public class AppointmentServiceModel : IMapFrom<Data.Models.Appointment>, IMapTo<Data.Models.Appointment>
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime Start { get; set; }

        public ApplicationUserServiceModel User { get; set; }

        public string UserId { get; set; }

        public bool IsApproved { get; set; }
    }
}
