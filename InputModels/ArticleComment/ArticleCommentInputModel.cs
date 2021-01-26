using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.ArticleComment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.InputModels.ArticleComment
{
    public class ArticleCommentInputModel : IMapTo<ArticleCommentServiceModel>
    {
        public string Content { get; set; }


        public int ArticleId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
