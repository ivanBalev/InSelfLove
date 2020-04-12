﻿namespace BDInSelfLove.Services.Data.Video
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Videos;

    public interface IVideoService
    {
        IQueryable<VideoServiceModel> GetAll(int? count = null);

        Task<int> CreateAsync(VideoServiceModel videoServiceModel);

        Task<bool> Delete(int id);
    }
}