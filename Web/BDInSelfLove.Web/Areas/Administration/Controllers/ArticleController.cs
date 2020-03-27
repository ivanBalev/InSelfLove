namespace BDInSelfLove.Web.Areas.Administration.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.CloudinaryService;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using BDInSelfLove.Web.ViewModels.Administration.Article;
    using BDInSelfLove.Web.ViewModels.Article;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class ArticleController : AdministrationController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IArticleService articleService;
        private readonly ICloudinaryService cloudinaryService;

        public ArticleController(UserManager<ApplicationUser> userManager, IArticleService articleService,
            ICloudinaryService cloudinaryService)
        {
            this.userManager = userManager;
            this.articleService = articleService;
            this.cloudinaryService = cloudinaryService;
        }


        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ArticleCreateInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            if (inputModel.ImageUrl == null)
            {
                var imageUrl = await this.cloudinaryService.UploadPicture(
                inputModel.Image, inputModel.Title);

                serviceModel.ImageUrl = imageUrl;
            }

            // TODO: Does this return postId?
            var postId = await this.articleService.CreateAsync(serviceModel);

            // TODO: Error handling
            return this.RedirectToAction("Single", "Article", new { area = string.Empty, id = postId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var articleServiceModel = await this.articleService.GetById(id);
            var articleEditViewModel =
                AutoMapperConfig.MapperInstance.Map<ArticleEditViewModel>(articleServiceModel);


            if (articleEditViewModel == null)
            {
                // TODO: Error Handling
                return this.Redirect("/");
            }

            return this.View(articleEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ArticleEditViewModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            if (inputModel.ImageUrl == null)
            {
                var imageUrl = await this.cloudinaryService.UploadPicture(
                inputModel.Image, inputModel.Title);

                serviceModel.ImageUrl = imageUrl;
            }

            await this.articleService.Edit(serviceModel);

            return this.RedirectToAction("Single", "Article", new { area = string.Empty, id = inputModel.Id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var articleServiceModel = await this.articleService.GetById(id);
            var articleDeleteModel =
                AutoMapperConfig.MapperInstance.Map<ArticleDeleteViewModel>(articleServiceModel);


            if (articleDeleteModel == null)
            {
                // TODO: Error Handling
                return this.Redirect("/");
            }

            return this.View(articleDeleteModel);
        }

        [HttpPost]
        [Route("/Article/Delete/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            await this.articleService.Delete(id);

            return this.Redirect("/");
        }
    }
}
