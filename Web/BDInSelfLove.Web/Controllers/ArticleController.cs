namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticleController : BaseController
    {
        private const int ArticlesPerPage = 6;

        private readonly IArticleService articleService;

        public ArticleController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        public async Task<IActionResult> All(int page = 1)
        {
            var serviceModel = this.articleService
                .GetAllPagination(ArticlesPerPage, (page - 1) * ArticlesPerPage);

            var pagesCount = (int)Math.Ceiling(this.articleService.GetAll().Count() / (decimal)ArticlesPerPage);
            var viewModel = new ArticlePaginationViewModel()
            {
                Articles = (await serviceModel.ToListAsync())
                .Select(a => AutoMapperConfig.MapperInstance.Map<BriefArticleInfoViewModel>(a))
                .ToList(),
                PagesCount = pagesCount == 0 ? 1 : pagesCount,
                CurrentPage = page,
            };

            return this.View(viewModel);
        }

        public async Task<IActionResult> Single(int id)
        {
            var viewModel = await this.GetViewModel(id);
            return this.View(viewModel);
        }

        private async Task<ArticleViewModel> GetViewModel(int id) =>
            AutoMapperConfig.MapperInstance.Map<ArticleViewModel>(await this.articleService.GetById(id));

        //private async Task<List<BriefArticleInfoViewModel>> GetViewModel()
        //{
        //    // Looks this horrible due to issues with query string created by ORM on matching ICollection
        //    return (await this.articleService
        //        .GetAll()
        //        .ToListAsync())
        //        .Select(a => AutoMapperConfig.MapperInstance.Map<BriefArticleInfoViewModel>(a))
        //        .ToList();
        //}
    }
}
