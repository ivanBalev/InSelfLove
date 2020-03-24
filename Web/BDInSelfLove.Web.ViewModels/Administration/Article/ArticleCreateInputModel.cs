namespace BDInSelfLove.Web.ViewModels.Administration.Article
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;

    public class ArticleCreateInputModel : IMapTo<ArticleServiceModel>
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
