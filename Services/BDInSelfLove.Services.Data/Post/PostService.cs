namespace BDInSelfLove.Services.Data.Post
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Services.Models.User;
    using BDInSelfLove.Web.InputModels.Forum.Category;
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

        public async Task<PostServiceModel> GetById(int id, int take, int skip = 0)
        {
            var post = await this.postRepository.All()
                .Where(p => p.Id == id)
                .Select(x => new PostServiceModel
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    Title = x.Title,
                    Content = x.Content,
                    CategoryId = x.CategoryId,
                    User = new ApplicationUserServiceModel
                    {
                        UserName = x.User.UserName,
                        ProfilePhoto = x.User.ProfilePhoto,
                        CreatedOn = x.User.CreatedOn,
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
                            ProfilePhoto = c.User.ProfilePhoto,
                            CreatedOn = x.User.CreatedOn,
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

            post.CommentsCount = post.Comments.Count();
            post.Comments = post.Comments.Skip(skip).Take(take).ToList();

            return post;
        }

        public IQueryable<PostServiceModel> Search(string searchTerm)
        {
            var searchItems = SearchHelpers.GetSearchItems(searchTerm);

            var posts = this.postRepository.All();

            foreach (var item in searchItems)
            {
                posts = posts.Where(p =>
                p.Content.ToLower().Contains(item) ||
                p.Title.ToLower().Contains(item));
            }

            return posts
                .Distinct()
                .OrderByDescending(p => p.CreatedOn)
                .To<PostServiceModel>();
        }
    }
}
