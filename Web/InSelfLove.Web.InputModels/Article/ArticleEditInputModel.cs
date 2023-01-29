using System;
using AutoMapper;
using InSelfLove.Services.Mapping;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InSelfLove.Web.InputModels.Article
{
    public class ArticleEditInputModel : ArticleCreateInputModel, IMapFrom<Data.Models.Article>
    {
        public int Id { get; set; }
    }
}
