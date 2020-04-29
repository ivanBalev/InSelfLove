﻿namespace BDInSelfLove.Web.ViewModels.Forum.Category
{
    using System;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;

    public class CategoryPostViewModel : IMapFrom<PostServiceModel>
    {
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Title { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public int CommentsCount { get; set; }
    }
}
