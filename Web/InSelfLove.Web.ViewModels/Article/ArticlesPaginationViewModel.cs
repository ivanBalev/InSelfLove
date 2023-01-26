namespace InSelfLove.Web.ViewModels.Article
{
    using System.Collections.Generic;

    using InSelfLove.Web.ViewModels.Home;
    using InSelfLove.Web.ViewModels.Pagination;

    public class ArticlesPaginationViewModel
    {
        public ICollection<ArticlePreviewViewModel> Articles { get; set; }

        public PaginationViewModel PaginationInfo { get; set; }
    }
}
