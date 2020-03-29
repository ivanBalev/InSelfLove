namespace BDInSelfLove.Web.Areas.ShopSPA.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : BaseShopController
    {
        public HomeController()
        {
        }

        public async Task<IActionResult> Index()
        {
            return this.View();
        }
    }
}
