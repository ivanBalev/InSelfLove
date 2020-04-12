namespace BDInSelfLove.Data.Models
{
    using System;

    using BDInSelfLove.Data.Common.Models;

    public class Product : BaseDeletableModel<int>
    {
        public Product()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string Picture { get; set; }

        public string SellerId { get; set; }

        public virtual ApplicationUser Seller { get; set; }

        public string BuyerId { get; set; }

        public virtual ApplicationUser Buyer { get; set; }

        public int? ProductTypeId { get; set; }

        public ProductCategory ProductType { get; set; }

    }
}
