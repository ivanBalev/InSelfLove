namespace BDInSelfLove.Web.ViewModels.Articles
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Articles;
    using Ganss.XSS;

    public class ArticleViewModel : IMapFrom<ArticleServiceModel>
    {
        public int Id { get; set; }

        public string UserUsername { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public string PreviewContent
        {
            get
            {
                var noHtmlTags = Regex.Replace(this.Content, @"<[^>]+>", string.Empty);

                if (noHtmlTags.Length > 200)
                {
                    noHtmlTags = noHtmlTags.Substring(0, 200) + "...";
                }

                return WebUtility.HtmlDecode(noHtmlTags);
            }
        }

        public string ImageUrl { get; set; }
    }
}
