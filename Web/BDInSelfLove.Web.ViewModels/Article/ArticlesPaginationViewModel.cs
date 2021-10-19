namespace BDInSelfLove.Web.ViewModels.Article
{
    using System.Collections.Generic;

    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Pagination;

    public class ArticlesPaginationViewModel
    {
        public ICollection<ArticlePreviewViewModel> Articles { get; set; }

        public PaginationViewModel PaginationInfo { get; set; }
    }
}
