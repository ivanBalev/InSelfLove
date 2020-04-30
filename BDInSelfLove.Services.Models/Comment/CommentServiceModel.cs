namespace BDInSelfLove.Services.Models.Comment
{
    using System;
    using System.Collections.Generic;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Services.Models.User;

    public class CommentServiceModel : IMapFrom<BDInSelfLove.Data.Models.Comment>, IMapTo<BDInSelfLove.Data.Models.Comment>
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? ParentCommentId { get; set; }

        public CommentServiceModel ParentComment { get; set; }

        public int? ParentPostId { get; set; }

        public PostServiceModel ParentPost { get; set; }

        public IList<CommentServiceModel> SubComments { get; set; }

        public ICollection<ReportServiceModel> Reports { get; set; }

        public ReportServiceModel Report { get; set; }
    }
}