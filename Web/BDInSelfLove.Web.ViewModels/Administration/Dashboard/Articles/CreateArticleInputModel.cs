namespace BDInSelfLove.Web.ViewModels.Administration.Dashboard.Articles
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Articles;

    public class CreateArticleInputModel : IMapTo<ArticleServiceModel>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Link to the image you'd like to use for your article")]
        public string ImageUrl { get; set; }
    }
}
