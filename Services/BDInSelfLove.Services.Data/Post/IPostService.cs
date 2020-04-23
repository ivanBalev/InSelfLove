namespace BDInSelfLove.Services.Data.Post
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Web.InputModels.Forum.Category;

    public interface IPostService
    {
        IQueryable<PostServiceModel> GetAll(int? count = null);

        Task<int> Create(PostServiceModel categoryServiceModel);

        Task<PostServiceModel> GetById(int id, int take, int skip = 0);

        IQueryable<PostServiceModel> SearchPosts(string searchTerm);

        ICollection<PostServiceModel> GetSortedPostsForCategory(int categoryId, CategorySortingInputModel sortingModel);

    }
}
