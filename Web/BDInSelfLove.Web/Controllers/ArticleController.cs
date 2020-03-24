namespace BDInSelfLove.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Article;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticleController : BaseController
    {
        private readonly IArticleService articleService;

        public ArticleController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        public async Task<IActionResult> All()
        {
            var viewModel = await this.articleService
                .GetAll()
                .To<ArticleViewModel>()
                .ToListAsync();

            return this.View(viewModel);
        }

        public async Task<IActionResult> Single(int id)
        {
            var model = await this.articleService
                .GetAll()
                .Where(a => a.Id == id)
                .To<ArticleViewModel>()
                .FirstOrDefaultAsync();

            return this.View(model);
        }
    }
}
