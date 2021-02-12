using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.VideoComment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.VideoComment
{
    public class VideoCommentEditInputModel : IMapTo<VideoCommentServiceModel>
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Comment must be longer than 2 symbols.")]
        public string Content { get; set; }
    }
}
