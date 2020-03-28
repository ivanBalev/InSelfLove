namespace BDInSelfLove.Services.Data.Post
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Services.Models.User;
    using Microsoft.EntityFrameworkCore;

    public class PostService : IPostService
    {
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Post> postRepository;
        private readonly ICommentService commentService;

        public PostService(IDeletableEntityRepository<BDInSelfLove.Data.Models.Post> postRepository, ICommentService commentService)
        {
            this.postRepository = postRepository;
            this.commentService = commentService;
        }

        public async Task<int> Create(PostServiceModel postServiceModel)
        {
            var post = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.Post>(postServiceModel);

            await this.postRepository.AddAsync(post);
            var result = await this.postRepository.SaveChangesAsync();

            return post.Id;
        }

        public IQueryable<PostServiceModel> GetAll(int? count = null)
        {
            IQueryable<BDInSelfLove.Data.Models.Post> query =
                this.postRepository.AllAsNoTracking().OrderByDescending(a => a.CreatedOn);

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return query.To<PostServiceModel>();
        }

        public async Task<PostServiceModel> GetById(int id)
        {
            // TODO: See what we can do about these requests - the one below is still int Red - slow. Used Automapper before that and had to wait 10+ seconds for response from server
            var post = await this.postRepository.All()
                .Select(x => new PostServiceModel
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    Title = x.Title,
                    Content = x.Content,
                    User = new ApplicationUserServiceModel
                    {
                        UserName = x.User.UserName,
                    },
                    UserId = x.UserId,
                    Comments = x.Comments.Select(c => new CommentServiceModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        ParentCommentId = c.ParentCommentId,
                        ParentPostId = c.ParentPostId,
                        UserId = c.UserId,
                        User = new ApplicationUserServiceModel
                        {
                            UserName = c.User.UserName,
                        },
                        CreatedOn = c.CreatedOn,
                    }).ToList(),
                }).FirstOrDefaultAsync();

            for (int i = 0; i < post.Comments.Count; i++)
            {
                var currentComment = post.Comments[i];
                await this.commentService.GetAllSubComments(currentComment, post);
            }

            // Remove all subcomments from the main post comments list
            post.Comments = post.Comments.Where(p => p.ParentCommentId == null).ToList();

            return post;
        }
    }
}
