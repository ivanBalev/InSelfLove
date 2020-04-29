namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Data.Post;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Web.ViewModels.Forum.Home.Category;
    using BDInSelfLove.Web.ViewModels.Forum.Search;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using SmartBreadcrumbs;

    public class HomeController : BaseForumController
    {
        private const string EmptySearchError = "Invalid search. Please try again.";
        private const int SearchItemsPerPage = 5;

        private readonly ICategoryService categoryService;
        private readonly IPostService postService;
        private readonly ICommentService commentService;

        public HomeController(
            ICategoryService categoryService,
            IPostService postService,
            ICommentService commentService)
        {
            this.categoryService = categoryService;
            this.postService = postService;
            this.commentService = commentService;
        }

        [DefaultBreadcrumb("Forum")]
        public async Task<IActionResult> Index()
        {
            var categories = await this.categoryService
              .GetHomeCategoryInfo()
              .To<HomeCategoryViewModel>()
              .ToListAsync();

            return this.View(categories);
        }

        [Breadcrumb("Search")]
        public async Task<ActionResult> Search(string term, string target, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                this.TempData["Error"] = EmptySearchError;
                return this.RedirectToAction("Index", "Home");
            }

            term = term.Trim();

            var postsQuery = this.postService.Search(term);
            var commentsQuery = this.commentService.Search(term);
            var categoriesQuery = this.categoryService.Search(term);

            var viewModel = new SearchViewModel
            {
                CurrentPage = page,
                SearchTerm = term,
                TotalCategoriesCount = await categoriesQuery.CountAsync(),
                TotalCommentsCount = await commentsQuery.CountAsync(),
                TotalPostsCount = await postsQuery.CountAsync(),
            };
            int totalItemsCount = term == "posts" ? viewModel.TotalPostsCount :
                                  term == "comments" ? viewModel.TotalCommentsCount :
                                                         viewModel.TotalCategoriesCount;

            switch (target)
            {
                case "posts":
                     await this.HandlePosts(term, page, viewModel, postsQuery);
                     break;
                case "comments":
                     await this.HandleComments(term, page, viewModel, commentsQuery);
                     break;
                case "categories":
                     await this.HandleCategories(term, page, viewModel, categoriesQuery);
                     break;
            }

            viewModel.PagesCount = (int)Math.Ceiling(totalItemsCount / (decimal)SearchItemsPerPage);

            if (viewModel.PagesCount == 0)
            {
                viewModel.PagesCount = 1;
            }

            var breadcrumb = new BreadcrumbNode("Search", "Search", "Home", null, null);
            this.ViewData["BreadcrumbNode"] = breadcrumb;

            return this.View(viewModel);
        }

        private async Task HandleCategories(string term, int page, SearchViewModel viewModel, IQueryable<CategoryServiceModel> categoriesQuery)
        {
            var categories = await categoriesQuery
                .To<SearchCategoryViewModel>()
                .Skip((page - 1) * SearchItemsPerPage)
                .Take(SearchItemsPerPage)
                .ToListAsync();

            viewModel.Categories = categories;
        }

        private async Task HandleComments(string term, int page, SearchViewModel viewModel, IQueryable<CommentServiceModel> commentsQuery)
        {
            var comments = await commentsQuery
                .To<SearchCommentViewModel>()
                .Skip((page - 1) * SearchItemsPerPage)
                .Take(SearchItemsPerPage)
                .ToListAsync();

            viewModel.Comments = comments;
        }

        private async Task HandlePosts(string term, int page, SearchViewModel viewModel, IQueryable<PostServiceModel> postsQuery)
        {
            var posts = await postsQuery
                .To<SearchPostViewModel>()
                .Skip((page - 1) * SearchItemsPerPage)
                .Take(SearchItemsPerPage)
                .ToListAsync();

            viewModel.Posts = posts;
        }
    }
}
