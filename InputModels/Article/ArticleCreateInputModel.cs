using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleCreateInputModel : IMapTo<ArticleServiceModel>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Display(Name = "Link to your article's image")]
        public string ImageUrl { get; set; }

        public IFormFile Image { get; set; }
    }
}
