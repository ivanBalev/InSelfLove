namespace BDInSelfLove.Data.Models
{
    using System;

    using BDInSelfLove.Data.Common.Models;

    public class ProductCategory : BaseDeletableModel<int>
    {
        public ProductCategory()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public string Name { get; set; }
    }
}
