﻿namespace BDInSelfLove.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using Microsoft.EntityFrameworkCore;

    public class ArticleService : IArticleService
    {
        private readonly IDeletableEntityRepository<Article> articleRepository;

        public ArticleService(IDeletableEntityRepository<Article> articleRepository)
        {
            this.articleRepository = articleRepository;
        }

        public async Task<int> CreateAsync(ArticleServiceModel articleServiceModel)
        {
            var article = AutoMapperConfig.MapperInstance.Map<Article>(articleServiceModel);

            await this.articleRepository.AddAsync(article);
            var result = await this.articleRepository.SaveChangesAsync();

            return article.Id;
        }

        public async Task<bool> Delete(int id)
        {
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == id);

            if (dbArticle == null)
            {
                throw new ArgumentNullException(nameof(dbArticle));
            }

            this.articleRepository.Delete(dbArticle);
            int result = await this.articleRepository.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> Edit(ArticleServiceModel articleServiceModel)
        {
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == articleServiceModel.Id);

            if (dbArticle == null)
            {
                throw new ArgumentNullException(nameof(dbArticle));
            }

            dbArticle.Title = articleServiceModel.Title;
            dbArticle.Content = articleServiceModel.Content;
            dbArticle.ImageUrl = articleServiceModel.ImageUrl;

            this.articleRepository.Update(dbArticle);
            int result = await this.articleRepository.SaveChangesAsync();

            return result;
        }

        public IQueryable<ArticleServiceModel> GetAll(int? latestArticlesCount = null)
        {
            IQueryable<Article> query = this.articleRepository.AllAsNoTracking().OrderByDescending(a => a.CreatedOn);

            if (latestArticlesCount.HasValue)
            {
                query = query.Take(latestArticlesCount.Value);
            }

            return query.To<ArticleServiceModel>();
        }

        public async Task<ArticleServiceModel> GetById(int id)
        {
            return await this.articleRepository.All()
               .To<ArticleServiceModel>()
               .SingleOrDefaultAsync(article => article.Id == id);
        }
    }
}