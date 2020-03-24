namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Web.ViewModels.Forum.Category;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class CategoryController : BaseForumController
    {
        private readonly ICategoryService categoryService;
        private readonly UserManager<ApplicationUser> userManager;

        public CategoryController(ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            this.categoryService = categoryService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(int id)
        {
            // TODO: Potentially consider either using plain AutoMapper or sending a direct SQL Command to Server
            var categoryServiceModel = await this.categoryService.GetById(id);
            var categoryViewModel = 
                AutoMapperConfig.MapperInstance.Map<CategoryViewModel>(categoryServiceModel);

            return this.View(categoryViewModel);
        }

        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public async Task<IActionResult> Create(CategoryCreateInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var serviceModel = AutoMapperConfig.MapperInstance.Map<CategoryServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var categoryId = await this.categoryService.Create(serviceModel);

            if (categoryId == 0)
            {
                this.TempData["Error"] = "Error";

                return this.View(inputModel);
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}
