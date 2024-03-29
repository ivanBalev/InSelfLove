﻿using InSelfLove.Services.Mapping;
using System.ComponentModel.DataAnnotations;

namespace InSelfLove.Web.InputModels.Comment
{
    public class CommentEditInputModel : IMapTo<Data.Models.Comment>
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Comment must be longer than 2 symbols.")]
        public string Content { get; set; }

        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

    }
}
