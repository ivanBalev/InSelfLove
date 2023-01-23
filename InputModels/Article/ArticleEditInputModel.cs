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
    }
}
