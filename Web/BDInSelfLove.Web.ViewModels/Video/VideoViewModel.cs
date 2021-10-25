namespace BDInSelfLove.Web.ViewModels.Video
{
    using System;
    using System.Collections.Generic;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Comment;

    public class VideoViewModel : IMapFrom<BDInSelfLove.Data.Models.Video>
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public IList<CommentViewModel> Comments { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
