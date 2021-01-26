namespace BDInSelfLove.Web.ViewModels.Home
{
    using System.Collections.Generic;

    public class HomeViewModel
    {
        public IEnumerable<BriefArticleInfoViewModel> LastArticles { get; set; }

        public BriefArticleInfoViewModel FeaturedArticle { get; set; }
    }
}
