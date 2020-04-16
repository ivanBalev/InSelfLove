namespace BDInSelfLove.Services.Data.Category
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Web.ViewModels.Forum.Category;
    using Microsoft.EntityFrameworkCore;

    public class CategoryService : ICategoryService
    {
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository;
        private readonly ICommentService commentService;

        public CategoryService(IDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository,
            ICommentService commentService)
        {
            this.categoryRepository = categoryRepository;
            this.commentService = commentService;
        }

        public async Task<int> Create(CategoryServiceModel categoryServiceModel)
        {
            var category = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.Category>(categoryServiceModel);

            await this.categoryRepository.AddAsync(category);
            await this.categoryRepository.SaveChangesAsync();

            return category.Id;
        }

        public async Task<CategoryServiceModel> GetById(int id)
        {
            return await this.categoryRepository.All()
                .Where(x => x.Id == id)
                .To<CategoryServiceModel>()
                .FirstOrDefaultAsync();
        }

        public IQueryable<CategoryServiceModel> GetHomeCategoryInfo()
        {
            // Horrendous request sent to server if Automapper is used.
            var result = this.categoryRepository.All().Select(c => new CategoryServiceModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                PostsCount = c.Posts.Count,
                CommentsCount = this.commentService.CommentsCountByCategoryId(c.Id),
                LastPost = c.Posts.OrderByDescending(p => p.CreatedOn)
                   .Select(p => new PostServiceModel
                   {
                       Title = p.Title,
                       Id = p.Id,
                       User = new Models.User.ApplicationUserServiceModel
                       {
                           UserName = p.User.UserName,
                       },
                       UserId = p.UserId,
                       CreatedOn = p.CreatedOn,
                   })
                   .FirstOrDefault(),
            });

            //var result1 = this.categoryRepository.All().Select(c => new CategoryServiceModel
            //{
            //    Id = c.Id,
            //    Name = c.Name,
            //    Description = c.Description,
            //    PostsCount = c.Posts.Count,
            //    CommentsCount = c.Posts.Sum(p => p.Comments.Count) -- why does this produce the need for DB UPDATE?,
            //    LastPost = c.Posts.OrderByDescending(p => p.CreatedOn)
            //       .Select(p => new PostServiceModel
            //       {
            //           Title = p.Title,
            //           Id = p.Id,
            //           User = new Models.User.ApplicationUserServiceModel
            //           {
            //               UserName = p.User.UserName,
            //           },
            //           UserId = p.UserId,
            //           CreatedOn = p.CreatedOn,
            //       })
            //       .FirstOrDefault(),
            //}).ToListAsync();

            return result;
        }
    }
}
