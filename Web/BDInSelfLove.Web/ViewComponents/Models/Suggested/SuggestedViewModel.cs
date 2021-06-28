using BDInSelfLove.Web.ViewModels.Home;
using BDInSelfLove.Web.ViewModels.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.ViewComponents.Models.Sidebar
{
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
