using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.ArticleComment;
using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BDInSelfLove.Web.ViewModels.ArticleComment
{
    public class ArticleCommentViewModel : IMapFrom<ArticleCommentServiceModel>
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }

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

        public IList<ArticleCommentViewModel> SubComments { get; set; }
    }
}
