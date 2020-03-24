namespace BDInSelfLove.Services.Data.Post
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using Microsoft.EntityFrameworkCore;

    public class PostService : IPostService
    {
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Post> postRepository;

        public PostService(IDeletableEntityRepository<BDInSelfLove.Data.Models.Post> postRepository)
        {
            this.postRepository = postRepository;
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
            return await this.postRepository.All()
                .To<PostServiceModel>()
                .SingleOrDefaultAsync(p => p.Id == id);
        }
    }
}
