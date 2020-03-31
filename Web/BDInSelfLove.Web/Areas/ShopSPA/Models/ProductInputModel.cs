using AutoMapper;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Product;

namespace BDInSelfLove.Web.Areas.ShopSPA.Models
{
    public class ProductInputModel : IMapTo<ProductServiceModel>/*, IHaveCustomMappings*/
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Price { get; set; }

        public string Picture { get; set; }

        //public string ProductCategory { get; set; }

        //public void CreateMappings(IProfileExpression configuration)
        //{
        //    configuration
        //        .CreateMap<ProductInputModel, ProductServiceModel>()
        //        .ForMember(destination => destination.ProductCategory,
        //                    opts => opts.MapFrom(origin => new ProductCategoryServiceModel { Name = origin.ProductCategory }));
        //}
    }
}
