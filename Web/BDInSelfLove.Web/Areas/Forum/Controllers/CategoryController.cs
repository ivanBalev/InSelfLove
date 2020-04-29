namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Web.InputModels.Administration.Category;
    using BDInSelfLove.Web.InputModels.Forum.Category;
    using BDInSelfLove.Web.ViewModels.Forum.Category;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using SmartBreadcrumbs;

    public class CategoryController : BaseForumController
    {
        private readonly ICategoryService categoryService;
        private readonly UserManager<ApplicationUser> userManager;

        public CategoryController(ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            this.categoryService = categoryService;
            this.userManager = userManager;
        }

        [Breadcrumb("Category")]
        public async Task<IActionResult> Index(int id, CategorySortingInputModel sortingModel)
        {
            var categoryServiceModel = await this.categoryService.GetById(id, sortingModel);
            var categoryViewModel = AutoMapperConfig.MapperInstance.Map<CategoryViewModel>(categoryServiceModel);

            var availableSortingCriteria = this.categoryService.GetAvailableSortingCriteria();

            categoryViewModel.CategorySorting = new CategorySortingInputModel
            {
                CategoryId = id,
                TimeCriteria = new List<SelectListItem>(availableSortingCriteria["TimeCriteria"].Select(x => new SelectListItem { Text = x, Value = x })),
                GroupingCriteria = new List<SelectListItem>(availableSortingCriteria["GroupingCriteria"].Select(x => new SelectListItem { Text = x, Value = x })),
                OrderingCriteria = new List<SelectListItem>(availableSortingCriteria["OrderingCriteria"].Select(x => new SelectListItem { Text = x, Value = x })),
            };

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
