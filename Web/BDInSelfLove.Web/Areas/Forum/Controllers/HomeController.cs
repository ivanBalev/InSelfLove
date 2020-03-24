namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Forum.Home.Category;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeController : BaseForumController
    {
        private readonly ICategoryService categoryService;

        public HomeController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await this.categoryService
              .GetHomeCategoryInfo()
              .To<HomeCategoryViewModel>()
              .ToListAsync();

            return this.View(categories);
        }
    }
}
