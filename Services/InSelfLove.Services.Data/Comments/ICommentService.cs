namespace InSelfLove.Services.Data.Comments
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;

    public interface ICommentService
    {
        Task Create(Comment commentServiceModel, string userId);

        IQueryable<Comment> GetById(int commentId);

        Task<int> Edit(Comment serviceModel, string userId);

        Task<int> Delete(int commentId, string userId, bool isUserAdmin);

        ICollection<Comment> ArrangeCommentHierarchy(ICollection<Comment> comments);

        Task SetCommentDepth(Comment comment);
    }
}
