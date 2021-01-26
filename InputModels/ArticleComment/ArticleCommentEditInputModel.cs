using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.ArticleComment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace BDInSelfLove.Web.InputModels.ArticleComment
{
    public class ArticleCommentEditInputModel : IMapTo<ArticleCommentServiceModel>
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int ArticleId { get; set; }
    }
}
