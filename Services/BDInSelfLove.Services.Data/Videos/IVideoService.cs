namespace BDInSelfLove.Services.Data.Videos
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface IVideoService
    {
        IQueryable<Video> GetAll(int? take = null, int skip = 0, string searchString = null);

        Task<string> Create(Video video);

        Task<int> Delete(int id);

        IQueryable<Video> GetSideVideos(int videosCount, int videoId = 0);

        Task<Video> GetBySlug(string slug);

    }
}
