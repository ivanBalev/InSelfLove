namespace BDInSelfLove.Services.Models.Articles
{
    using System;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Users;

    public class ArticleServiceModel : IMapTo<Article>, IMapFrom<Article>
    {
        public ArticleServiceModel()
        {
            this.User = new ApplicationUserServiceModel();
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }
    }
}
