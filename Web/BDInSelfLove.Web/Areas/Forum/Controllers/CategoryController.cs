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

        [HttpGet]
        [Breadcrumb("Category")]
        public async Task<IActionResult> Index(int id)
        {
            // TODO: Potentially consider either using plain AutoMapper or sending a direct SQL Command to Server
            var categoryServiceModel = await this.categoryService.GetById(id);
            var categoryViewModel = AutoMapperConfig.MapperInstance.Map<CategoryViewModel>(categoryServiceModel);

            var availableSortingCriteria = new Dictionary<string, List<string>>
            {
                { "TimeCriteria", new List<string> { "all posts", "day", "month", "year" } },
                { "GroupingCriteria",  new List<string> { "date created", "author", "replies", "topic" } },
                { "OrderingCriteria", new List<string> { "descending", "ascending" } },
            };

            categoryViewModel.CategorySorting = new CategorySortingModel
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
