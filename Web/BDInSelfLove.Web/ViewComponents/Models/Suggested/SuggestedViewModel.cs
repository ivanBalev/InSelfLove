namespace BDInSelfLove.Web.ViewComponents.Models.Sidebar
{
    using System.Collections.Generic;

    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Video;

    public class SuggestedViewModel
    {
        public SuggestedViewModel()
        {
            this.Articles = new List<ArticlePreviewViewModel>();
            this.Videos = new List<VideoPreviewViewModel>();
        }

        public List<ArticlePreviewViewModel> Articles { get; set; }

        public List<VideoPreviewViewModel> Videos { get; set; }
    }
}
