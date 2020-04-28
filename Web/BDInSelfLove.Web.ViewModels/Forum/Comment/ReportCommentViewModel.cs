using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    public class ReportCommentViewModel : IMapFrom<CommentServiceModel>
    {
        [Required]
        public int Id { get; set; }

        
        public string UserUsername { get; set; }

        [Required]
        [MinLength(30)]
        public string Reason { get; set; }
    }
}
