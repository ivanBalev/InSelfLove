using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Comment
{
    public interface ICommentService
    {
        Task<int> CreateAsync(CommentServiceModel categoryServiceModel);

        IQueryable<CommentServiceModel> GetAll(int parentId, string parentType);
    }
}
