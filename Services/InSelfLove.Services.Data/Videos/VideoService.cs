namespace InSelfLove.Services.Data.Videos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Comments;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.EntityFrameworkCore;
    using NinjaNye.SearchExtensions;

    public class VideoService : IVideoService
    {
        private readonly IDeletableEntityRepository<Video> videosRepository;
        private readonly ICommentService commentService;

        public VideoService(
            IDeletableEntityRepository<Video> videosRepository,
            ICommentService commentService)
        {
            this.videosRepository = videosRepository;
            this.commentService = commentService;
        }

        public async Task<string> Create(Video video)
        {
            // Validate input
            if (video == null ||
              string.IsNullOrEmpty(video.Title) || string.IsNullOrWhiteSpace(video.Title) ||
              string.IsNullOrEmpty(video.Url) || string.IsNullOrWhiteSpace(video.Url) ||
              string.IsNullOrEmpty(video.AssociatedTerms) || string.IsNullOrWhiteSpace(video.AssociatedTerms))
            {
                throw new ArgumentException(nameof(video));
            }

            // Create new video
            await this.videosRepository.AddAsync(video);
            await this.videosRepository.SaveChangesAsync();

            // Return slug to controller for redirect
            return video.Slug;
        }

        public async Task<Video> GetBySlug(string slug, string userTimezone)
        {
            if (slug == null)
            {
                return null;
            }

            var video = await this.videosRepository.All()
               .Where(a => a.Slug.Equals(slug.ToLower()))
              .Select(x => new Video
              {
                  // Ensure query efficiency by picking only relevant data
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
                      VideoId = c.VideoId,
                      ParentCommentId = c.ParentCommentId,
                      CreatedOn = c.CreatedOn,

                      // Instantiate list for ArrangeCommentHierarchy
                      SubComments = new List<Comment>(),
                  })),
              })
                .FirstOrDefaultAsync();

            // No need to proceed further if video does not exist
            if (video == null)
            {
                return null;
            }


            // Create comments hierarchy tree and order by date
            // We get a unidimensional list of comments from db
            // Its nesting needs to be sorted out for the client
            video.Comments = this.commentService.ArrangeCommentHierarchy(video.Comments);

            // Adjust comments CreatedOn to user's local time
            foreach (var comment in video.Comments)
            {
                comment.CreatedOn = TimezoneHelper.ToLocalTime(comment.CreatedOn, userTimezone);
            }

            return video;
        }

        public IQueryable<Video> GetAll(int? take = null, int skip = 0, string searchString = null)
        {
            var query = this.videosRepository.All();

            // If client is searching for particular phrases (Request coming from Search controller)
            if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrWhiteSpace(searchString))
            {
                // Leave only meaningful words
                var searchItems = SearchHelper.GetSearchItems(searchString);

                // Build query with videos which associated terms or title match the search terms
                query = query.Search(x => x.AssociatedTerms.ToLower(), x => x.Title.ToLower()).Containing(searchItems);
            }

            // Order data & skip if using search pagination
            query = query.Distinct().OrderByDescending(c => c.CreatedOn).Skip(skip);

            // if take has no value, then skip will be 0 as well => no pagination used
            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        public async Task<int> Delete(int id)
        {
            var dbVideo = await this.videosRepository.All().SingleOrDefaultAsync(a => a.Id == id);

            if (dbVideo == null)
            {
                throw new ArgumentException(nameof(dbVideo));
            }

            this.videosRepository.Delete(dbVideo);
            return await this.videosRepository.SaveChangesAsync();
        }

        public async Task<IList<Video>> GetSideVideos(int videosCount, DateTime date)
        {
            // Take videos older than current one
            var videos = await this.videosRepository.All()
              .Where(v => DateTime.Compare(v.CreatedOn, date) < 0)
              .OrderByDescending(v => v.CreatedOn)
              .Take(videosCount)
              .ToListAsync();

            // If they're not enough (video is one of the older ones)
            if (videos.Count < videosCount)
            {
                var additionalVideosNeeded = videosCount - videos.Count;

                // Get the number of videos needed from the newer ones
                videos.AddRange(await this.videosRepository.All()
               .Where(a => DateTime.Compare(a.CreatedOn, date) > 0)
               .OrderBy(v => v.CreatedOn)
               .Take(additionalVideosNeeded)
               .ToListAsync());
            }

            return videos;
        }

        public async Task<string> Edit(Video video)
        {
            // Validate input
            if (video == null ||
              string.IsNullOrEmpty(video.Title) || string.IsNullOrWhiteSpace(video.Title) ||
              string.IsNullOrEmpty(video.Url) || string.IsNullOrWhiteSpace(video.Url) ||
              string.IsNullOrEmpty(video.AssociatedTerms) || string.IsNullOrWhiteSpace(video.AssociatedTerms))
            {
                throw new ArgumentException(nameof(video));
            }

            // Get video, make sure it's unique & throw error if it doesn't exist
            var dbVideo = await this.videosRepository.All().SingleOrDefaultAsync(v => v.Id == video.Id);
            if (dbVideo == null)
            {
                throw new ArgumentException(nameof(dbVideo));
            }

            // Update video
            dbVideo.Title = video.Title;
            dbVideo.Url = video.Url;
            dbVideo.AssociatedTerms = video.AssociatedTerms;
            dbVideo.Slug = video.Slug;

            // Make sure date is valid
            // If no value is given to DateTime from client
            // It defaults to 01.01.0001
            if (video.CreatedOn.Year > 1)
            {
                dbVideo.CreatedOn = video.CreatedOn;
            }

            // Update video in db
            this.videosRepository.Update(dbVideo);
            await this.videosRepository.SaveChangesAsync();

            // Return slug to controller for redirect
            return dbVideo.Slug;
        }

        public IQueryable<Video> GetById(int id)
        {
            return this.videosRepository.All().Where(v => v.Id == id);
        }
    }
}
