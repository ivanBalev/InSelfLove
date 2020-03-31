using BDInSelfLove.Services.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Product
{
    public interface IProductService
    {
        Task<ProductServiceModel> Create(ProductServiceModel productServiceModel);

        IQueryable<ProductServiceModel> GetAll(int? count = null);
    }
}
