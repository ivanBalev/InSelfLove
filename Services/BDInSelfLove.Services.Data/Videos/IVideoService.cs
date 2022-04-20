namespace BDInSelfLove.Services.Data.Videos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface IVideoService
    {
        IQueryable<Video> GetAll(int? take = null, int skip = 0, string searchString = null);

        Task<string> Create(Video video);

        Task<int> Delete(int id);

        Task<IList<Video>> GetSideVideos(int videosCount, DateTime date);

        IQueryable<Video> GetById(int id);

        Task<Video> GetBySlug(string slug);

        Task<string> Edit(Video video);
    }
}
