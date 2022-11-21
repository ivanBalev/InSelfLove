namespace BDInSelfLove.Web.InputModels.Appointment
{
    using System.ComponentModel.DataAnnotations;

    public class AppointmentInputModel
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }
    }
}
