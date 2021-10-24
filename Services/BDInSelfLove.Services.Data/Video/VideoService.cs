namespace BDInSelfLove.Services.Data.Video
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.CommentService;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Video;
    using BDInSelfLove.Services.Models.Videos;
    using Microsoft.EntityFrameworkCore;

    public class VideoService : IVideoService
    {
        private readonly IDeletableEntityRepository<Video> videosRepository;
        private readonly ICommentService commentService;

        public VideoService(IDeletableEntityRepository<Video> videosRepository, ICommentService commentService)
        {
            this.videosRepository = videosRepository;
            this.commentService = commentService;
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

        public async Task<Video> GetBySlug(string slug)
        {
            var video = await this.videosRepository.All()
               .Where(a => a.Title.ToLower() == slug.Replace('-', ' '))
              .Select(x => new Video
              {
                  Id = x.Id,
                  Url = x.Url,
                  Title = x.Title,
                  CreatedOn = x.CreatedOn,
                  Comments = new List<Comment>(x.Comments.Select(c => new Comment
                  {
                      Id = c.Id,
                      Content = c.Content,
                      UserId = c.UserId,
                      User = new ApplicationUser
                      {
                          UserName = c.User.UserName,
                          ProfilePhoto = c.User.ProfilePhoto,
                      },
                      ArticleId = c.ArticleId,
                      ParentCommentId = c.ParentCommentId,
                      CreatedOn = c.CreatedOn,
                      SubComments = new List<Comment>(),
                  })),
              })
                .FirstOrDefaultAsync();

            video.Comments = this.commentService.ArrangeCommentHierarchy(video.Comments);
            return video;
        }

        public IQueryable<Video> GetAll(int? take = null, int skip = 0, string searchString = null)
        {
            var query = this.videosRepository.All();

            if (searchString != null)
            {
                var searchItems = SearchHelpers.GetSearchItems(searchString);
                foreach (var item in searchItems)
                {
                    // TODO: need to check why I needed this
                    var tempItem = item;

                    query = query.Where(v =>
                    v.AssociatedTerms.ToLower().Contains(tempItem) ||
                    v.Title.ToLower().Contains(tempItem));
                }
            }

            query = query.Distinct().OrderByDescending(a => a.CreatedOn).Skip(skip);

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
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

        public IQueryable<Video> GetSideVideos(int videosCount, int videoId = 0)
        {
            var videos = this.videosRepository.All()
               .Where(v => v.Id != videoId)
               .OrderByDescending(a => a.CreatedOn)
               .Take(videosCount);

            return videos;
        }
    }
}
