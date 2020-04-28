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
        [MinLength(20)]
        public string Title { get; set; }

        [Required]
        [MinLength(30)]
        public string Content { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
