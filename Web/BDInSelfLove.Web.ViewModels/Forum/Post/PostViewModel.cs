namespace BDInSelfLove.Web.ViewModels.Forum.Post
{
    using System;
    using System.Collections.Generic;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using Ganss.XSS;

    public class PostViewModel : IMapFrom<PostServiceModel>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public DateTime UserCreatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public ICollection<PostCommentViewModel> Comments { get; set; }

        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }
    }
}
