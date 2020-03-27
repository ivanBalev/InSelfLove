namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Post;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Web.ViewModels.Forum.Post;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class PostController : BaseForumController
    {
        private readonly IPostService postService;
        private readonly UserManager<ApplicationUser> userManager;

        public PostController(IPostService postService, UserManager<ApplicationUser> userManager)
        {
            this.postService = postService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(int id)
        {
            var serviceModel = await this.postService
                .GetById(id);

            var viewModel = AutoMapperConfig.MapperInstance.Map<PostViewModel>(serviceModel);

            return this.View(serviceModel);
        }

        public IActionResult Create(int categoryId)
        {
            categoryId = 1;

            this.ViewData["CategoryId"] = categoryId;

            return this.View();
        }

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
