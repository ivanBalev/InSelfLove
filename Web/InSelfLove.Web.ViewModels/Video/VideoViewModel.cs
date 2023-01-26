namespace InSelfLove.Web.ViewModels.Video
{
    using System;
    using System.Collections.Generic;

    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.ViewModels.Comment;

    public class VideoViewModel : IMapFrom<InSelfLove.Data.Models.Video>
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public IList<CommentViewModel> Comments { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
