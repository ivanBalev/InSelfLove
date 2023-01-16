namespace BDInSelfLove.Web.ViewModels.Comment
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using BDInSelfLove.Services.Mapping;
    using Ganss.Xss;

    public class CommentViewModel : IMapFrom<Data.Models.Comment>
    {
        public int Id { get; set; }

        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public string ContentWithNoTags
        {
            get
            {
                return Regex.Replace(this.Content, @"<[^>]+>", string.Empty);
            }
        }

        public int? ParentCommentId { get; set; }

        public IList<CommentViewModel> SubComments { get; set; }
    }
}
