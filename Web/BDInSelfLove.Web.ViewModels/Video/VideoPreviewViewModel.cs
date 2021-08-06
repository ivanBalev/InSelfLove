using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Video;
using BDInSelfLove.Services.Models.Videos;
using System;

namespace BDInSelfLove.Web.ViewModels.Video
{
    public class VideoPreviewViewModel : IMapFrom<VideoPreviewServiceModel>, IMapFrom<VideoServiceModel>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Slug => this.Title.ToLower().Replace(' ', '-');

        public string Url { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
