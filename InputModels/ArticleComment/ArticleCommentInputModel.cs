using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.ArticleComment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.ArticleComment
{
    public class ArticleCommentInputModel : IMapTo<ArticleCommentServiceModel>
    {
        [Required]
        [MinLength(2, ErrorMessage = "Comment must be longer than 2 symbols")]
        public string Content { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
