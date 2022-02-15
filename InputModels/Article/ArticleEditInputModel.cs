using System;
using AutoMapper;
using BDInSelfLove.Services.Mapping;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleEditInputModel : ArticleCreateInputModel, IMapFrom<Data.Models.Article>
    {
        public int Id { get; set; }

        public byte[] PreviewImageBlob { get; set; }

        public override void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<ArticleEditInputModel, Data.Models.Article>().ForMember(
                m => m.PreviewImageBlob,
                opt => opt.MapFrom(x => Convert.FromBase64String(x.PreviewImage
                .Split(',', StringSplitOptions.RemoveEmptyEntries)[1])));
        }
    }
}
