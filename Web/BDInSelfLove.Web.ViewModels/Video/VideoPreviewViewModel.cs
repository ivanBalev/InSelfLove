namespace BDInSelfLove.Web.ViewModels.Video
{
    using System;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Data.Models;

    public class VideoPreviewViewModel : IMapFrom<Video>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Url { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
