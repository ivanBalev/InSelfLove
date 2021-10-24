namespace BDInSelfLove.Web.ViewModels.Video
{
    using System;

    using BDInSelfLove.Services.Mapping;

    public class VideoPreviewViewModel : IMapFrom<BDInSelfLove.Data.Models.Video>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Slug => this.Title.ToLower().Replace(' ', '-');

        public string Url { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
