namespace BDInSelfLove.Services.Data.Category
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Web.ViewModels.Forum.Category;
    using Microsoft.EntityFrameworkCore;

    public class CategoryService : ICategoryService
    {
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository;

        public CategoryService(IDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<int> Create(CategoryServiceModel categoryServiceModel)
        {
            var category = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.Category>(categoryServiceModel);

            await this.categoryRepository.AddAsync(category);
            await this.categoryRepository.SaveChangesAsync();

            return category.Id;
        }

        public IQueryable<CategoryServiceModel> GetAll()
        {
            IQueryable<BDInSelfLove.Data.Models.Category> query =
                this.categoryRepository.All().OrderBy(a => a.Name);

            return query.To<CategoryServiceModel>();
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

            return result;
        }
    }
}
