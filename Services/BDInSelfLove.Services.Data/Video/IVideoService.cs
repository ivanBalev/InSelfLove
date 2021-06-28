namespace BDInSelfLove.Services.Data.Video
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Services.Models.Video;
    using BDInSelfLove.Services.Models.Videos;

    public interface IVideoService
    {
        IQueryable<VideoServiceModel> GetAll(int? count = null);

        Task<ICollection<VideoPreviewServiceModel>> GetAllPagination(int take, int skip = 0);

        Task<int> CreateAsync(VideoServiceModel videoServiceModel);

        Task<bool> Delete(int id);

        IQueryable<VideoPreviewServiceModel> GetSideVideos(int videosCount, int videoId = 0);

        Task<VideoServiceModel> GetById(int id);

    }
}
