namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Post;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Web.ViewModels.Forum.Post;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using SmartBreadcrumbs;

    public class PostController : BaseForumController
    {
        private const int CommentsPerPage = 5;

        private readonly IPostService postService;
        private readonly UserManager<ApplicationUser> userManager;

        public PostController(IPostService postService, UserManager<ApplicationUser> userManager)
        {
            this.postService = postService;
            this.userManager = userManager;
        }

        [HttpGet]
        [Breadcrumb("Post")]
        public async Task<IActionResult> Index(int id, int page = 1)
        {
            var serviceModel = await this.postService
                .GetById(id, CommentsPerPage, (page - 1) * CommentsPerPage);

            var pagesCount = (int)Math.Ceiling(serviceModel.CommentsCount / (decimal)CommentsPerPage);
            var viewModel = AutoMapperConfig.MapperInstance.Map<PostViewModel>(serviceModel);
            viewModel.CurrentPage = page;
            viewModel.PagesCount = pagesCount;

            if (viewModel.PagesCount == 0)
            {
                viewModel.PagesCount = 1;
            }

            var parentCategory = new BreadcrumbNode("Category", "Index", "Category", new { id = serviceModel.CategoryId });
            var childPost = new BreadcrumbNode("Post", "Index", "Post", null, parentCategory);
            this.ViewData["BreadcrumbNode"] = childPost;

            return this.View(viewModel);
        }

        [Authorize]
        public IActionResult Create(int categoryId)
        {
            return this.View(new PostCreateInputModel { CategoryId = categoryId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(PostCreateInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var serviceModel = AutoMapperConfig.MapperInstance.Map<PostServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var postId = await this.postService.Create(serviceModel);

            if (postId == 0)
            {
                this.TempData["Error"] = "Error";

                return this.View(inputModel);
            }

            return this.RedirectToAction("Index", new { id = postId });
        }
    }
}
