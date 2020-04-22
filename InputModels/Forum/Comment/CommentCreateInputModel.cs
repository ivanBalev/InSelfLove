using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Forum.Comment
{
    public class CommentCreateInputModel : IMapTo<CommentServiceModel>
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public int ParentPostId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
