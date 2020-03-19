namespace BDInSelfLove.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Articles;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticlesController : BaseController
    {
        private readonly IArticleService articleService;

        public ArticlesController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        public async Task<IActionResult> IndexAsync()
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
