namespace BDInSelfLove.Services.Data.Video
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Video;
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

        public async Task<string> CreateAsync(VideoServiceModel videoServiceModel)
        {
            var video = AutoMapperConfig.MapperInstance.Map<Video>(videoServiceModel);

            await this.videosRepository.AddAsync(video);
            await this.videosRepository.SaveChangesAsync();

            return video.Title.ToLower().Replace(' ', '-');
        }

        public async Task<VideoServiceModel> GetById(int id)
        {
            var video = await this.videosRepository.All()
               .Where(a => a.Id == id)
               .Include(v => v.User)
               .Include(v => v.Comments.OrderByDescending(c => c.CreatedOn))
               .To<VideoServiceModel>()
               .FirstOrDefaultAsync();

            foreach (var comment in video.Comments)
            {
                comment.SubComments.Clear();
            }

            foreach (var comment in video.Comments)
            {
                if (comment.ParentCommentId != null)
                {
                    var parentComment = video.Comments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                    parentComment.SubComments.Add(comment);
                }
            }

            video.Comments = video.Comments.Where(v => v.ParentCommentId == null).ToList();

            return video;
        }

        public async Task<VideoServiceModel> GetBySlug(string slug)
        {
            var video = await this.videosRepository.All()
               .Where(a => a.Title.ToLower() == slug.Replace('-', ' '))
               .Include(v => v.User)
               .Include(v => v.Comments.OrderByDescending(c => c.CreatedOn))
               .To<VideoServiceModel>()
               .FirstOrDefaultAsync();

            foreach (var comment in video.Comments)
            {
                comment.SubComments.Clear();
            }

            foreach (var comment in video.Comments)
            {
                if (comment.ParentCommentId != null)
                {
                    var parentComment = video.Comments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                    parentComment.SubComments.Add(comment);
                }
            }

            video.Comments = video.Comments.Where(v => v.ParentCommentId == null).ToList();

            return video;
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

        public async Task<ICollection<VideoPreviewServiceModel>> GetAllPagination(int take = DefaultVideosPerPage, int skip = 0)
        {
            var videos = await this.videosRepository
                               .All()
                               .Include(a => a.User)
                               .Include(a => a.Comments)
                               .OrderByDescending(a => a.CreatedOn)
                               .Skip(skip)
                               .Take(take)
                               .To<VideoPreviewServiceModel>()
                               .ToListAsync();

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

        public IQueryable<VideoPreviewServiceModel> GetSideVideos(int videosCount, int videoId = 0)
        {
            var videos = this.videosRepository.All()
               .Where(v => v.Id != videoId)
               .OrderByDescending(a => a.CreatedOn)
               .Take(videosCount)
               .To<VideoPreviewServiceModel>();

            return videos;
        }
    }
}
