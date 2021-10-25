namespace BDInSelfLove.Services.Data.CommentService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface ICommentService
    {
        Task<int> Create(Comment commentServiceModel);

        IQueryable<Comment> GetAllByArticleId(int articleId);

        IQueryable<Comment> GetAllByVideoId(int videoId);

        IQueryable<Comment> GetById(int commentId);

        Task<int> Edit(Comment serviceModel, string userId);

        Task<int> Delete(int commentId, string userId, bool isUserAdmin);

        ICollection<Comment> ArrangeCommentHierarchy(ICollection<Comment> comments);

    }
}
