using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Services.Models.Search;
using BDInSelfLove.Services.Models.User;
using BDInSelfLove.Services.Models.Video;
using BDInSelfLove.Services.Models.Videos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Search
{
    public class SearchService : ISearchService
    {
        private const int DefaultVideosPerPage = 3;
        private const int DefaultArticlesPerPage = 6;


        private readonly IDeletableEntityRepository<Article> articleRepository;
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Video> videoRepository;

        public SearchService(
            IDeletableEntityRepository<Article> articleRepository,
            IDeletableEntityRepository<BDInSelfLove.Data.Models.Video> videoRepository)
        {
            this.articleRepository = articleRepository;
            this.videoRepository = videoRepository;
        }

        public async Task<IndexSearchServiceModel> Index(string searchTerm)
        {
            var searchItems = SearchHelpers.GetSearchItems(searchTerm);

            var articles = this.articleRepository.All();
            var videos = this.videoRepository.All();

            foreach (var item in searchItems)
            {
                var tempItem = item;

                articles = articles.Where(p =>
                p.Content.ToLower().Contains(tempItem) ||
                p.Title.ToLower().Contains(tempItem));

                videos = videos.Where(p =>
                p.AssociatedTerms.ToLower().Contains(tempItem) ||
                p.Title.ToLower().Contains(tempItem));
            }

            var serviceModel = new IndexSearchServiceModel
            {
                Articles = await articles.Distinct().Take(DefaultArticlesPerPage).To<ArticlePreviewServiceModel>().ToListAsync(),
                Videos = await videos.Distinct().Take(DefaultVideosPerPage).To<VideoPreviewServiceModel>().ToListAsync(),
                ArticlesCount = await articles.CountAsync(),
                VideosCount = await videos.CountAsync(),
            };

            return serviceModel;
        }

        public async Task<ArticlesSearchServiceModel> GetArticles(string searchTerm, int take = DefaultArticlesPerPage, int skip = 0)
        {
            var searchItems = SearchHelpers.GetSearchItems(searchTerm);

            var articlesQuery = this.articleRepository.All();

            foreach (var item in searchItems)
            {
                var tempItem = item;

                articlesQuery = articlesQuery.Where(p =>
                p.Content.ToLower().Contains(tempItem) ||
                p.Title.ToLower().Contains(tempItem));
            }

            var articlesCount = await articlesQuery.CountAsync();
            var articles = await articlesQuery.Skip(skip).Take(take)
                               .Select(a => new ArticlePreviewServiceModel
                               {
                                  Id = a.Id,
                                  Title = a.Title,
                                  CreatedOn = a.CreatedOn,
                                  Content = a.Content,
                                  ImageUrl = a.ImageUrl,
                               })
                               .ToListAsync();

            return new ArticlesSearchServiceModel
            {
                Articles = articles,
                ArticlesCount = articlesCount,
            };
        }

        public async Task<VideosSearchServiceModel> GetVideos(string searchTerm, int take = DefaultVideosPerPage, int skip = 0)
        {
            var searchItems = SearchHelpers.GetSearchItems(searchTerm);

            var videosQuery = this.videoRepository.All();

            foreach (var item in searchItems)
            {
                var tempItem = item;

                videosQuery = videosQuery.Where(p =>
                p.AssociatedTerms.ToLower().Contains(tempItem) ||
                p.Title.ToLower().Contains(tempItem));
            }

            var videosCount = await videosQuery.Distinct().CountAsync();
            var videos = await videosQuery.Skip(skip).Take(take)
                               .To<VideoPreviewServiceModel>()
                               .ToListAsync();

            return new VideosSearchServiceModel
            {
                Videos = videos,
                VideosCount = videosCount,
            };
        }
    }
}
