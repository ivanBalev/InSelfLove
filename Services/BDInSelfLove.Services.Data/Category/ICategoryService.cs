namespace BDInSelfLove.Services.Data.Category
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Web.InputModels.Forum.Category;
    using BDInSelfLove.Web.ViewModels.Forum.Category;

    public interface ICategoryService
    {
        Task<int> Create(CategoryServiceModel categoryServiceModel);

        Task<CategoryServiceModel> GetById(int id, CategorySortingInputModel sortingModel);

        IQueryable<CategoryServiceModel> GetHomeCategoryInfo();
    }
}
