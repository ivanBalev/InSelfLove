using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Post;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Forum.Post
{
    public class PostCreateInputModel : IMapTo<PostServiceModel>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
