namespace BDInSelfLove.Web.ViewModels.Home
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;

    public class ArticlePreviewViewModel : IMapFrom<Article>
    {
        private const int PreviewContentLength = 320;
        private const int WordsCount = 60;

        public int Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string UserUsername { get; set; }

        public string PreviewContent
        {
            get
            {
                var noHtmlTags = Regex.Replace(this.Content, @"<[^>]+>", string.Empty);
                noHtmlTags = WebUtility.HtmlDecode(noHtmlTags);

                string[] contentArr = noHtmlTags.Split(' ');
                if (contentArr.Length > WordsCount)
                {
                    string[] shortenedContent = new string[WordsCount];
                    Array.Copy(contentArr, shortenedContent, WordsCount);
                    return string.Join(' ', shortenedContent) + "...";
                }

                return noHtmlTags;
            }
        }

        public string PreviewImageUrl { get; set; }

        public string ImageUrl { get; set; }
    }
}
