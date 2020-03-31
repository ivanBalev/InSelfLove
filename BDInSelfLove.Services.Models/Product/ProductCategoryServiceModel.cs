namespace BDInSelfLove.Services.Models.Product
{
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;

    public class ProductCategoryServiceModel : IMapFrom<ProductCategory>, IMapTo<ProductCategory>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
