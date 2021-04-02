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
                noHtmlTags = WebUtility.HtmlDecode(noHtmlTags);

                if ((noHtmlTags + this.Title).Length > 180)
                {
                    noHtmlTags = noHtmlTags.Substring(0, 180 - this.Title.Length) + "...";
                }
                //TODO: This needs to work but with array spliy by space, not single characters. Otherwise - BUGS when visualizing
                return noHtmlTags;
            }
        }

        public string ImageUrl { get; set; }
    }
}