using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Video
{
    public class VideoPaginationViewModel
    {
        public ICollection<VideoViewModel> Videos { get; set; }

        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }
    }
}
