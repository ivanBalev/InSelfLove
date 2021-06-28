using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Services.Models.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Comment
{
    public class CommentServiceModel : IMapFrom<Data.Models.Comment>, IMapTo<Data.Models.Comment>
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUserServiceModel User { get; set; }

        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

        public int? ParentCommentId { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual CommentServiceModel ParentComment { get; set; }

        public virtual IList<CommentServiceModel> SubComments { get; set; }
    }
}
