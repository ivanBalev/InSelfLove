using InSelfLove.Data.Models;
using System.Threading.Tasks;

namespace InSelfLove.Web.Controllers.Helpers
{
    public interface IAppointmentEmailHelper
    {
        Task SendEmail(
           Appointment appointment,
           bool fromAdmin,
           string status,
           ApplicationUser admin,
           ApplicationUser user);
    }
}
