namespace BDInSelfLove.Web.ViewModels.Article
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;

    public class ArticleEditViewModel : IMapTo<ArticleServiceModel>, IMapFrom<ArticleServiceModel>
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Link to the image you'd like to use for your article")]
        public string ImageUrl { get; set; }
    }
}
