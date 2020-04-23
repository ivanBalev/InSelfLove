namespace BDInSelfLove.Services.Models.Post
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.User;

    public class PostServiceModel : IMapTo<BDInSelfLove.Data.Models.Post>, IMapFrom<Data.Models.Post>
    {
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }

        public IList<CommentServiceModel> Comments { get; set; }

        public int CategoryId { get; set; }

        public CategoryServiceModel Category { get; set; }

        public int CommentsCount { get; set; }
    }
}