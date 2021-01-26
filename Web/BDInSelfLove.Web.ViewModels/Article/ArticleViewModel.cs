namespace BDInSelfLove.Web.ViewModels.Article
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.RegularExpressions;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using BDInSelfLove.Web.ViewModels.ArticleComment;
    using Ganss.XSS;

    public class ArticleViewModel : IMapFrom<ArticleServiceModel>
    {
        public ArticleViewModel()
        {
            this.ArticleComments = new List<ArticleCommentViewModel>();
        }

        public int Id { get; set; }

        public string UserUsername { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public string ImageUrl { get; set; }

        public string PreviewContent
        {
            get
            {
                var noHtmlTags = Regex.Replace(this.Content, @"<[^>]+>", string.Empty);

                if ((noHtmlTags + this.Title.Length).Length > 200)
                {
                    noHtmlTags = noHtmlTags.Substring(0, 200 - this.Title.Length) + "...";
                }

                return WebUtility.HtmlDecode(noHtmlTags);
            }
        }

        public ICollection<ArticleCommentViewModel> ArticleComments { get; set; }
    }
}
