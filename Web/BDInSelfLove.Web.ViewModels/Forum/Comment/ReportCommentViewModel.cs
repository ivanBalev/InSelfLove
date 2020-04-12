using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    // TODO: MAKE SURE ALL VALIDATION IS PRESENT EVERYWHERE!!!
    public class ReportCommentViewModel : IMapFrom<CommentServiceModel>
    {
        public int Id { get; set; }

        public string UserUsername { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}
