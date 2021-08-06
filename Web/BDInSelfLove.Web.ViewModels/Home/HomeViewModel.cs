namespace BDInSelfLove.Web.ViewModels.Home
{
    using BDInSelfLove.Web.ViewModels.Video;
    using System.Collections.Generic;

    public class HomeViewModel
    {
        public IEnumerable<ArticlePreviewViewModel> LastArticles { get; set; }

        public ArticlePreviewViewModel FeaturedArticle { get; set; }

        public VideoPreviewViewModel FeaturedVideo { get; set; }
    }
}
