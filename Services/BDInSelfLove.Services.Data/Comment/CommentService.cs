namespace BDInSelfLove.Services.Data.Comment
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;

    public class CommentService : ICommentService
    {
        private readonly IDeletableEntityRepository<Comment> commentRepository;

        public CommentService(IDeletableEntityRepository<Comment> commentRepository)
        {
            this.commentRepository = commentRepository;
        }

        public async Task<int> CreateAsync(CommentServiceModel categoryServiceModel)
        {
            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(categoryServiceModel);

            await this.commentRepository.AddAsync(comment);
            var result = await this.commentRepository.SaveChangesAsync();

            return comment.Id;
        }

        public IQueryable<CommentServiceModel> GetAll(int parentId, string parentType)
        {
            // TODO: Switch by parent type and extract ony the appropriate comments

            IQueryable<Comment> query = this.commentRepository.All();
                //.Where(c => )
                //OrderByDescending(a => a.CreatedOn);


            return query.To<CommentServiceModel>();
        }
    }
}
