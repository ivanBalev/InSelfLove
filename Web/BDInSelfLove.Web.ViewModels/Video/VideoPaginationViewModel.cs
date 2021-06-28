using BDInSelfLove.Web.ViewModels.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Video
{
    public class VideoPaginationViewModel
    {
        public ICollection<VideoPreviewViewModel> Videos { get; set; }

        public PaginationViewModel PaginationInfo { get; set; }
    }
}
