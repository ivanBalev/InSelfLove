namespace InSelfLove.Web.ViewModels.Video
{
    using System.Collections.Generic;

    using InSelfLove.Web.ViewModels.Pagination;

    public class VideosPaginationViewModel
    {
        public ICollection<VideoPreviewViewModel> Videos { get; set; }

        public PaginationViewModel PaginationInfo { get; set; }
    }
}
