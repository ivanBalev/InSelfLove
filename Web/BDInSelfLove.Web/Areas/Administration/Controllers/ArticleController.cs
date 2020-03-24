namespace BDInSelfLove.Web.Areas.Administration.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
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

        public ArticleController(UserManager<ApplicationUser> userManager, IArticleService articleService)
        {
            this.userManager = userManager;
            this.articleService = articleService;
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
        public async Task<IActionResult> Edit(ArticleEditViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleServiceModel>(model);
            serviceModel.UserId = user.Id;

            await this.articleService.Edit(serviceModel);

            return this.RedirectToAction("Single", "Article", new { area = string.Empty, id = model.Id });
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
