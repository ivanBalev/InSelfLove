namespace BDInSelfLove.Web.ViewModels.Article
{
    using System;
    using System.Collections.Generic;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using BDInSelfLove.Web.ViewModels.Comment;
    using Ganss.XSS;

    public class ArticleViewModel : IMapFrom<Article>, IMapFrom<ArticleServiceModel>
    {
        public ArticleViewModel()
        {
            this.Comments = new List<CommentViewModel>();
        }

        public int Id { get; set; }

        public string UserUsername { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public string ImageUrl { get; set; }

        public ICollection<CommentViewModel> Comments { get; set; }
    }
}
