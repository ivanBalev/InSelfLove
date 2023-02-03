using System;
using InSelfLove.Services.Mapping;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoMapper;
using System.IO;

namespace InSelfLove.Web.InputModels.Article
{
    public class ArticleCreateInputModel : IMapTo<Data.Models.Article>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string PreviewImageUrl { get; set; }

        public IFormFile Image { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public IFormFile PreviewImage { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Slug => Regex.Replace(this.Title.ToLower().Trim().Replace(' ', '-'), "[^a-zа-я0-9-_~]+", string.Empty);
    }
}
