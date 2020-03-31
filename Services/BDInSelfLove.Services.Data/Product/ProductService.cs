using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Product
{
    public class ProductService : IProductService
    {
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Product> productRepository;

        public ProductService(IDeletableEntityRepository<BDInSelfLove.Data.Models.Product> productRepository)
        {
            this.productRepository = productRepository;
        }

        public async Task<ProductServiceModel> Create(ProductServiceModel productServiceModel)
        {
            var product = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.Product>(productServiceModel);

            await this.productRepository.AddAsync(product);
            await this.productRepository.SaveChangesAsync();

            return AutoMapperConfig.MapperInstance.Map<ProductServiceModel>(product);
        }

        public IQueryable<ProductServiceModel> GetAll(int? count = null)
        {
            IQueryable<BDInSelfLove.Data.Models.Product> query =
             this.productRepository.AllAsNoTracking().OrderByDescending(a => a.CreatedOn);

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return query.To<ProductServiceModel>();
        }
    }
}
