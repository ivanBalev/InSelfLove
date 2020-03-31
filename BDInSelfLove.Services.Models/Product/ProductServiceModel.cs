using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Product
{
    public class ProductServiceModel : IMapFrom<Data.Models.Product>, IMapTo<Data.Models.Product>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string Picture { get; set; }

        public string SellerId { get; set; }

        public ApplicationUserServiceModel Seller { get; set; }

        public string BuyerId { get; set; }

        public ApplicationUserServiceModel Buyer { get; set; }

        public int ProductCategoryId { get; set; }

        public ProductCategoryServiceModel ProductCategory { get; set; }

    }
}
