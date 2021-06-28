namespace BDInSelfLove.Services.Models.Article
{
    using System;
    using System.Collections.Generic;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.User;

    public class ArticleServiceModel : IMapTo<Article>, IMapFrom<Article>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }

        public ICollection<CommentServiceModel> Comments { get; set; }
    }
}
