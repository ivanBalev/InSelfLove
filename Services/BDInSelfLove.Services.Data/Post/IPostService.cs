namespace BDInSelfLove.Services.Data.Post
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Post;

    public interface IPostService
    {
        IQueryable<PostServiceModel> GetAll(int? count = null);

        Task<int> Create(PostServiceModel categoryServiceModel);

        Task<PostServiceModel> GetById(int id, int take, int skip = 0);
    }
}
