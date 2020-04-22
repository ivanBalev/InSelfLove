using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Administration.Article
{
    public class ArticleEditInputModel : IMapTo<ArticleServiceModel>, IMapFrom<ArticleServiceModel>
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Display(Name = "Link to the image you'd like to use for your article.")]
        public string ImageUrl { get; set; }

        [Display(Name = "Or, upload an image, if you prefer.")]

        public IFormFile Image { get; set; }
    }
}
