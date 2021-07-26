using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleEditInputModel : IMapTo<ArticleServiceModel>, IMapFrom<ArticleServiceModel>
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public IFormFile Image { get; set; }
    }
}
