namespace BDInSelfLove.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    // DEPENDENCY INJECTION WITH FILTERS
    //[TypeFilter(typeof(AddHeaderActionFilterAttribute))]
    public class HomeController : BaseController
    {
        private const int IndexArticlesCount = 4;

        private readonly IArticleService articleService;

        public HomeController(IArticleService articleService)
        {
            this.articleService = articleService;

            // TODO: Payments will need deployment. Otherwise we can't receive response from epay about payment status.
        }

        //[AddHeaderAsyncActionFilter]
        //// SERVICEFILTER ALLOWS US TO CONTROL HOW WE INSTANTIATE THE FILTER (SINGLETON, TRANSIENT, SCOPED) IN THE FILE STARTUP.CS
        ////[ServiceFilter(typeof(AddHeaderAsyncActionFilterAttribute))]
        //[MyAuthorizationFilter]
        //[MyExceptionFilter]
        //[MyResourceFilter]
        //[MyResultFilter]
        public async Task<IActionResult> Index()
        {
            var lastArticles = await this.articleService
               .GetAll(IndexArticlesCount)
               .To<ArticleViewModel>()
               .ToListAsync();

            var viewModel = new HomeViewModel
            {
                LastArticles = lastArticles.Skip(1),
                FeaturedArticle = lastArticles[0],
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
            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

    }
}
