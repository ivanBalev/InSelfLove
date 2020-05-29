namespace BDInSelfLove.Services.Data.Category
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Web.InputModels.Forum.Category;

    public interface ICategoryService
    {
        Task<int> Create(CategoryServiceModel categoryServiceModel);

        Task<CategoryServiceModel> GetById(int id, CategorySortingInputModel sortingModel);

        IQueryable<CategoryServiceModel> GetHomeCategoryInfo();

        IQueryable<CategoryServiceModel> Search(string searchTerm);
    }
}
