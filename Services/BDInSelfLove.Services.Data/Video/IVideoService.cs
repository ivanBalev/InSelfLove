namespace BDInSelfLove.Services.Data.Video
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Models.Video;
    using BDInSelfLove.Services.Models.Videos;

    public interface IVideoService
    {
        IQueryable<Video> GetAll(int? take = null, int skip = 0, string searchString = null);

        Task<string> CreateAsync(VideoServiceModel videoServiceModel);

        Task<bool> Delete(int id);

        IQueryable<Video> GetSideVideos(int videosCount, int videoId = 0);

        Task<VideoServiceModel> GetById(int id);

        Task<Video> GetBySlug(string slug);

    }
}
