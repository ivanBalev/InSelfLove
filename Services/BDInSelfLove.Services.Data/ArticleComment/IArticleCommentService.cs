using BDInSelfLove.Services.Models.ArticleComment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.ArticleComment
{
    public interface IArticleCommentService
    {
        Task<int> Create(ArticleCommentServiceModel articleCommentServiceModel);

        IQueryable<ArticleCommentServiceModel> GetAllByArticleId(int articleId);

        IQueryable<ArticleCommentServiceModel> GetById(int commentId);

        Task<int> Edit(ArticleCommentServiceModel serviceModel);
    }
}
