using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Video;
using System;

namespace BDInSelfLove.Web.ViewModels.Video
{
    public class VideoPreviewViewModel : IMapFrom<VideoPreviewServiceModel>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
