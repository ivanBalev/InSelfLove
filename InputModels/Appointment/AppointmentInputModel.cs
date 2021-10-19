namespace BDInSelfLove.Web.InputModels.Appointment
{
    using System.ComponentModel.DataAnnotations;

    public class AppointmentInputModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(30)]
        public string Description { get; set; }
    }
}
