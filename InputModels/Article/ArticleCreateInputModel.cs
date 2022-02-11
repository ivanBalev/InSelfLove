using System;
using AutoMapper;
using BDInSelfLove.Services.Mapping;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleCreateInputModel : IMapTo<Data.Models.Article>, IMapFrom<Data.Models.Article>, IHaveCustomMappings
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public IFormFile Image { get; set; }

        public string PreviewImage { get; set; }

        public string Slug => this.Title.ToLower().Replace(' ', '-');

        public virtual void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<ArticleCreateInputModel, Data.Models.Article>().ForMember(
                m => m.PreviewImageBlob,
                opt => opt.MapFrom(x => Convert.FromBase64String(x.PreviewImage
                .Split(',', StringSplitOptions.RemoveEmptyEntries)[1])));
        }
    }
}
