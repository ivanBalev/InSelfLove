namespace BDInSelfLove.Web.ViewModels.Home
{
    using BDInSelfLove.Web.ViewModels.Video;
    using System.Collections.Generic;

    public class HomeViewModel
    {
        public IEnumerable<BriefArticleInfoViewModel> LastArticles { get; set; }

        public BriefArticleInfoViewModel FeaturedArticle { get; set; }

        public VideoViewModel FeaturedVideo { get; set; }
    }
}
