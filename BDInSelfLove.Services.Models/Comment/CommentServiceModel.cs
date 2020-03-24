using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Services.Models.Post;
using BDInSelfLove.Services.Models.User;
using BDInSelfLove.Services.Models.Videos;
using System;
using System.Collections.Generic;

namespace BDInSelfLove.Services.Models.Comment
{
    public class CommentServiceModel : IMapFrom<BDInSelfLove.Data.Models.Comment>, IMapTo<BDInSelfLove.Data.Models.Comment>
    {
        public CommentServiceModel()
        {
            this.SubComments = new List<CommentServiceModel>();
        }

        public int Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedOn { get; set; }

        public  ApplicationUserServiceModel User { get; set; }

        public int? ParentCommentId { get; set; }

        public int? ParentVideoId { get; set; }

        public int? ParentArticleId { get; set; }

        public int? ParentPostId { get; set; }

        public CommentServiceModel ParentComment { get; set; }

        public VideoServiceModel ParentVideo { get; set; }

        public ArticleServiceModel ParentArticle { get; set; }

        public PostServiceModel Post { get; set; }

        public IList<CommentServiceModel> SubComments { get; set; }
    }
}
