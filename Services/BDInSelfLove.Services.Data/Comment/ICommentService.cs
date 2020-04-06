using BDInSelfLove.Services.Models.Comment;
using BDInSelfLove.Services.Models.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Comment
{
    public interface ICommentService
    {
        Task<int> Create(CommentServiceModel categoryServiceModel);

        IQueryable<CommentServiceModel> GetAll(int parentId, string parentType);

        Task GetAllSubComments(CommentServiceModel comment, PostServiceModel post);
    }
}
