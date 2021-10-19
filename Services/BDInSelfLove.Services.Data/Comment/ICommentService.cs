namespace BDInSelfLove.Services.Data.CommentService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Models.Comment;

    public interface ICommentService
    {
        Task<int> Create(Comment commentServiceModel);

        IQueryable<Comment> GetAllByArticleId(int articleId);

        IQueryable<Comment> GetAllByVideoId(int videoId);

        IQueryable<Comment> GetById(int commentId);

        Task<int> Edit(Comment serviceModel);

        Task<int> Delete(int commentId);

        ICollection<CommentServiceModel> ArrangeCommentHierarchy(ICollection<CommentServiceModel> comments);

    }
}
