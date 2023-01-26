namespace InSelfLove.Web.ViewComponents.Models.Sidebar
{
    using System.Collections.Generic;

    using InSelfLove.Web.ViewModels.Home;
    using InSelfLove.Web.ViewModels.Video;

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
