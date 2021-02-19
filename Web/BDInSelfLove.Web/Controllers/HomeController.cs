namespace BDInSelfLove.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeController : BaseController
    {
        private const int IndexArticlesCount = 4;

        private readonly IArticleService articleService;

        public HomeController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        public async Task<IActionResult> Index()
        {
            // Get data
            var serviceData = await this.articleService
               .GetAll(IndexArticlesCount).ToListAsync();

            // Parse
            var lastArticles = serviceData.Select(x => AutoMapperConfig.MapperInstance.Map<BriefArticleInfoViewModel>(x)).ToList();

            // Create view model
            var viewModel = new HomeViewModel
            {
                FeaturedArticle = lastArticles[0],
                LastArticles = lastArticles.Skip(1).ToList(),
            };

            return this.View(viewModel);
        }

        public IActionResult Contact()
        {
            return this.View();
        }

        [Authorize]
        public IActionResult Appointment()
        {
            return this.View();
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var context = this.HttpContext;

            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
