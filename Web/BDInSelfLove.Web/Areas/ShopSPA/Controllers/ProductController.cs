namespace BDInSelfLove.Web.Areas.ShopSPA.Controllers
{
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Product;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Product;
    using BDInSelfLove.Web.Areas.ShopSPA.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IProductService productService;

        public ProductController(UserManager<ApplicationUser> userManager, IProductService productService)
        {
            this.userManager = userManager;
            this.productService = productService;
        }

        [HttpPost]
        // TODO: make separate view model and return that instead of the service model
        public async Task<ActionResult<ProductServiceModel>> Create(ProductInputModel input)
        {
            var user = await this.userManager.GetUserAsync(this.User);

            var serviceModel = AutoMapperConfig.MapperInstance.Map<ProductServiceModel>(input);
            serviceModel.SellerId = user.Id;

            //if (inputModel.ImageUrl == null)
            //{
            //    var imageUrl = await this.cloudinaryService.UploadPicture(
            //    inputModel.Image, inputModel.Title);

            //    serviceModel.ImageUrl = imageUrl;
            //}

            var createdProduct = await this.productService.Create(serviceModel);

            // TODO: Error handling

            // TODO: make separate view model
            return createdProduct;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<ProductServiceModel>>> GetAll()
        {
            var productServiceModel = await this.productService.GetAll()
                .ToListAsync();

            return productServiceModel;
        }
    }
}
