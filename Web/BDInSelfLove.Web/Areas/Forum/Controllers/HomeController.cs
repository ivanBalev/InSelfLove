namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Data.Post;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Forum.Home.Category;
    using BDInSelfLove.Web.ViewModels.Forum.Search;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeController : BaseForumController
    {
        private const int SearchPostsPerPage = 5;

        private readonly ICategoryService categoryService;
        private readonly IPostService postService;

        public HomeController(ICategoryService categoryService, IPostService postService)
        {
            this.categoryService = categoryService;
            this.postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await this.categoryService
              .GetHomeCategoryInfo()
              .To<HomeCategoryViewModel>()
              .ToListAsync();

            return this.View(categories);
        }

        [HttpGet]
        public async Task<ActionResult> Search(string term, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return RedirectToAction("Index", "Home");
            }

            term = term.Trim();

            var postsQuery = this.postService.SearchPosts(
                term).To<SearchPostViewModel>();

            var totalPostsCount = postsQuery.Count();
            var posts = await postsQuery.Skip((page - 1) * SearchPostsPerPage).Take(SearchPostsPerPage)
                .ToListAsync();

            var pagesCount = (int)Math.Ceiling(totalPostsCount / (decimal)SearchPostsPerPage);

            // create the view model
            var viewModel = new SearchViewModel
            {
                Posts = posts,
                PagesCount = pagesCount,
                CurrentPage = page,
                SearchTerm = term,
            };

            return View(viewModel);
        }
    }
}
