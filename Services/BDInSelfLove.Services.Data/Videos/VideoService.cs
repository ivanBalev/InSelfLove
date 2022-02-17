﻿namespace BDInSelfLove.Services.Data.Videos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Comments;
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
            if (video == null ||
              string.IsNullOrEmpty(video.Title) || string.IsNullOrWhiteSpace(video.Title) ||
              string.IsNullOrEmpty(video.Url) || string.IsNullOrWhiteSpace(video.Url) ||
              string.IsNullOrEmpty(video.AssociatedTerms) || string.IsNullOrWhiteSpace(video.AssociatedTerms))
            {
                throw new ArgumentException(nameof(video));
            }

            await this.videosRepository.AddAsync(video);
            await this.videosRepository.SaveChangesAsync();
            return video.Title.ToLower().Replace(' ', '-');
        }

        public async Task<Video> GetBySlug(string slug)
        {
            if (slug == null)
            {
                return null;
            }

            var video = await this.videosRepository.All()
               .Where(a => a.Slug.Equals(slug.ToLower()))
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
                      VideoId = c.VideoId,
                      ParentCommentId = c.ParentCommentId,
                      CreatedOn = c.CreatedOn,
                      SubComments = new List<Comment>(),
                  })),
              })
                .FirstOrDefaultAsync();

            if (video == null)
            {
                return null;
            }

            video.Comments = this.commentService.ArrangeCommentHierarchy(video.Comments);
            return video;
        }

        public IQueryable<Video> GetAll(int? take = null, int skip = 0, string searchString = null)
        {
            var query = this.videosRepository.All();

            if (searchString != null)
            {
                var searchItems = SearchHelper.GetSearchItems(searchString);
                query = query.Search(x => x.AssociatedTerms.ToLower(), x => x.Title.ToLower()).Containing(searchItems);
            }

            query = query.Distinct().OrderByDescending(c => c.CreatedOn.Date)
               .ThenByDescending(c => c.CreatedOn.TimeOfDay).Skip(skip);

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

        public IQueryable<Video> GetSideVideos(int videosCount, int videoId = 0)
        {
            var videos = this.videosRepository.All()
               .Where(v => v.Id != videoId)
                 .OrderByDescending(c => c.CreatedOn.Date)
               .ThenByDescending(c => c.CreatedOn.TimeOfDay)
               .Take(videosCount);

            return videos;
        }

        public async Task<string> Edit(Video video)
        {
            if (video == null ||
              string.IsNullOrEmpty(video.Title) || string.IsNullOrWhiteSpace(video.Title) ||
              string.IsNullOrEmpty(video.Url) || string.IsNullOrWhiteSpace(video.Url) ||
              string.IsNullOrEmpty(video.AssociatedTerms) || string.IsNullOrWhiteSpace(video.AssociatedTerms))
            {
                throw new ArgumentException(nameof(video));
            }

            var dbVideo = await this.videosRepository.All().SingleOrDefaultAsync(v => v.Id == video.Id);

            if (dbVideo == null)
            {
                throw new ArgumentException(nameof(dbVideo));
            }

            dbVideo.Title = video.Title;
            dbVideo.Url = video.Url;
            dbVideo.AssociatedTerms = video.AssociatedTerms;
            dbVideo.Slug = video.Slug;
            dbVideo.CreatedOn = video.CreatedOn;

            this.videosRepository.Update(dbVideo);
            await this.videosRepository.SaveChangesAsync();

            return dbVideo.Slug;
        }

        public IQueryable<Video> GetById(int id)
        {
            return this.videosRepository.All().Where(v => v.Id == id);
        }
    }
}
