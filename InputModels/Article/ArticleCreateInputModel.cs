using AutoMapper;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleCreateInputModel : IMapTo<ArticleServiceModel>, IHaveCustomMappings
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public IFormFile Image { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<ArticleCreateInputModel, ArticleServiceModel>().ForMember(
                m => m.Title,
                opt => opt.MapFrom(x => x.Title.Trim()));
        }
    }
}
