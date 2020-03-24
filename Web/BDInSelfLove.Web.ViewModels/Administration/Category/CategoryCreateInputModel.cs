namespace BDInSelfLove.Web.ViewModels.Administration.Category
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;

    public class CategoryCreateInputModel : IMapTo<CategoryServiceModel>
    {
        [Required]
        public string Name { get; set; }
    }
}
