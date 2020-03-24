namespace BDInSelfLove.Web.ViewModels.Forum.Category
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;

    public class CategoryCreateInputModel : IMapTo<CategoryServiceModel>
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}