using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BDInSelfLove.Web.ViewModels.Home
{
    public class ArticlePreviewViewModel : IMapFrom<ArticleServiceModel>, IMapFrom<ArticlePreviewServiceModel>
    {
        private const int PreviewContentLength = 320;

        public int Id { get; set; }

        public string Title { get; set; }

        public string Slug => this.Title.ToLower().Replace(' ', '-');

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string UserUsername { get; set; }

        public string PreviewContent
        {
            get
            {
                var noHtmlTags = Regex.Replace(this.Content, @"<[^>]+>", string.Empty);
                noHtmlTags = WebUtility.HtmlDecode(noHtmlTags);

                if ((noHtmlTags + this.Title).Length > PreviewContentLength)
                {
                    noHtmlTags = noHtmlTags.Substring(0, PreviewContentLength - this.Title.Length) + "...";
                }

                return noHtmlTags;
            }
        }

        public string ImageUrl { get; set; }
    }
}