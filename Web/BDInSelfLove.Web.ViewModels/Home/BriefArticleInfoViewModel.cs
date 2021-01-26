using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BDInSelfLove.Web.ViewModels.Home
{
    public class BriefArticleInfoViewModel : IMapFrom<ArticleServiceModel>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string UserUsername { get; set; }

        public string PreviewContent
        {
            get
            {
                var noHtmlTags = Regex.Replace(this.Content, @"<[^>]+>", string.Empty);

                if ((noHtmlTags + this.Title).Length > 200)
                {
                    noHtmlTags = noHtmlTags.Substring(0, 200 - this.Title.Length) + "...";
                }

                return WebUtility.HtmlDecode(noHtmlTags);
            }
        }

        public string ImageUrl { get; set; }
    }
}