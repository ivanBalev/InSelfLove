namespace BDInSelfLove.Services.Data.Video
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;
    using Microsoft.EntityFrameworkCore;

    public class VideoService : IVideoService
    {
        private const int DefaultVideosPerPage = 3;

        private readonly IDeletableEntityRepository<Video> videosRepository;

        public VideoService(IDeletableEntityRepository<Video> videosRepository)
        {
            this.videosRepository = videosRepository;
        }

        public async Task<int> CreateAsync(VideoServiceModel videoServiceModel)
        {
            var video = AutoMapperConfig.MapperInstance.Map<Video>(videoServiceModel);

            await this.videosRepository.AddAsync(video);
            await this.videosRepository.SaveChangesAsync();

            return video.Id;
        }

        public IQueryable<VideoServiceModel> GetAll(int? latestVideosCount = null)
        {
            IQueryable<Video> query = this.videosRepository.AllAsNoTracking();

            if (latestVideosCount.HasValue)
            {
                query = query
                    .OrderByDescending(a => a.CreatedOn)
                    .Take(latestVideosCount.Value);
            }

            return query.To<VideoServiceModel>();
        }

        public async Task<ICollection<VideoServiceModel>> GetAllPagination(int take = DefaultVideosPerPage, int skip = 0)
        {
            var videos = await this.videosRepository
                               .All()
                               .Include(a => a.User)
                               .Include(a => a.VideoComments)
                               .OrderByDescending(a => a.CreatedOn)
                               .Skip(skip)
                               .Take(take)
                               .To<VideoServiceModel>()
                               .ToListAsync();

            foreach (var video in videos)
            {
                foreach (var comment in video.VideoComments)
                {
                    comment.SubComments.Clear();

                    if (comment.ParentCommentId != null)
                    {
                        var parentComment = video.VideoComments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                        parentComment.SubComments.Add(comment);
                    }
                }

                video.VideoComments = video.VideoComments.Where(vc => vc.ParentCommentId == null).ToList();
            }

            return videos;
        }

        public async Task<bool> Delete(int id)
        {
            var dbVideo = await this.videosRepository.All().SingleOrDefaultAsync(a => a.Id == id);

            if (dbVideo == null)
            {
                throw new ArgumentNullException(nameof(dbVideo));
            }

            this.videosRepository.Delete(dbVideo);
            int result = await this.videosRepository.SaveChangesAsync();

            return result > 0;
        }
    }
}
