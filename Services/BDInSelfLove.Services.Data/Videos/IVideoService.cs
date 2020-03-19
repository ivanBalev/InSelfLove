namespace BDInSelfLove.Services.Data.Videos
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Videos;

    public interface IVideoService
    {
        IQueryable<VideoServiceModel> GetAll(int? count = null);

        Task<int> CreateAsync(VideoServiceModel videoServiceModel);
    }
}
