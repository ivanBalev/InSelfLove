namespace BDInSelfLove.Web.ViewModels.Video
{
    using System.Collections.Generic;

    using BDInSelfLove.Web.ViewModels.Pagination;

    public class VideosPaginationViewModel
    {
        public ICollection<VideoPreviewViewModel> Videos { get; set; }

        public PaginationViewModel PaginationInfo { get; set; }
    }
}
