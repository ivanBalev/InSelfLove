﻿namespace BDInSelfLove.Services.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Article;

    public interface IArticleService
    {
        IQueryable<ArticleServiceModel> GetAll(int? count = null);

        IQueryable<ArticlePreviewServiceModel> GetAllPagination(int take, int skip = 0);

        Task<int> CreateAsync(ArticleServiceModel articleServiceModel);

        Task<ArticleServiceModel> GetById(int id);

        Task<int> Edit(ArticleServiceModel productServiceModel);

        Task<bool> Delete(int id);

        IQueryable<ArticlePreviewServiceModel> GetSideArticles(int articlesCount, int articleId = 0);
    }
}
