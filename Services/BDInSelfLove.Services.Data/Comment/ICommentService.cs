using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.CommentService
{
    public interface ICommentService
    {
        Task<int> Create(CommentServiceModel commentServiceModel);

        IQueryable<CommentServiceModel> GetAllByArticleId(int articleId);

        IQueryable<CommentServiceModel> GetAllByVideoId(int videoId);

        IQueryable<CommentServiceModel> GetById(int commentId);

        Task<int> Edit(CommentServiceModel serviceModel);

        Task<int> Delete(int commentId);

        ICollection<CommentServiceModel> ArrangeCommentHierarchy(ICollection<CommentServiceModel> comments);

    }
}
