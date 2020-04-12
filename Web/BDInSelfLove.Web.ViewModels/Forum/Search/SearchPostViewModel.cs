using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Post;
using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Search
{
    public class SearchPostViewModel : IMapFrom<PostServiceModel>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);
    }
}
