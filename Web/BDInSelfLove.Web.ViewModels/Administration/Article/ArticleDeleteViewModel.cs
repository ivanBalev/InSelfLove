namespace BDInSelfLove.Web.ViewModels.Administration.Article
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;

    public class ArticleDeleteViewModel : IMapFrom<ArticleServiceModel>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        [Display(Name = "Link to the image you'd like to use for your article")]
        public string ImageUrl { get; set; }
    }
}
