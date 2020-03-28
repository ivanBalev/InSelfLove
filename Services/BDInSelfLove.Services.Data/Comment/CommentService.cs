namespace BDInSelfLove.Services.Data.Comment
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;
    using Microsoft.EntityFrameworkCore;

    public class CommentService : ICommentService
    {
        private const string ParentComment = "comment";
        private const string ParentArticle = "article";
        private const string ParentVideo = "video";
        private const string ParentPost = "post";


        private readonly IDeletableEntityRepository<Comment> commentRepository;

        public CommentService(IDeletableEntityRepository<Comment> commentRepository)
        {
            this.commentRepository = commentRepository;
        }

        public async Task<int> Create(CommentServiceModel categoryServiceModel)
        {
            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(categoryServiceModel);

            await this.commentRepository.AddAsync(comment);
            await this.commentRepository.SaveChangesAsync();

            return comment.Id;
        }

        public IQueryable<CommentServiceModel> GetAll(int parentId, string parentType)
        {
            IQueryable<Comment> query = this.commentRepository.All();

            switch (parentType.ToLower())
            {
                case ParentComment: return query.Where(c => c.ParentArticleId != null).To<CommentServiceModel>();
                case ParentArticle: return query.Where(c => c.ParentArticleId != null).To<CommentServiceModel>();
                case ParentVideo: return query.Where(c => c.ParentVideoId != null).To<CommentServiceModel>();
                case ParentPost: return query.Where(c => c.ParentPostId != null).To<CommentServiceModel>();
            }

            // TODO: Redo this alternative return. It shouldn't be possible to receive anything other than the 4 parent types.
            return query.To<CommentServiceModel>();
        }

        public async Task<CommentServiceModel> GetAllSubComments(CommentServiceModel comment, PostServiceModel post)
        {
            var subComments = post.Comments.Where(c => c.ParentCommentId == comment.Id).ToList();

            comment.SubComments = subComments;

            for (var i = 0; i < subComments.Count; i++)
            {
                await this.GetAllSubComments(subComments[i], post);
            }

            return comment;
        }
    }
}
