using BDInSelfLove.Services.Mapping;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleCreateInputModel : IMapTo<Data.Models.Article>, IMapFrom<Data.Models.Article>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public IFormFile Image { get; set; }
    }
}
