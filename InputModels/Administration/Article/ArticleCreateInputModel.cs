using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Administration.Article
{
    public class ArticleCreateInputModel : IMapTo<ArticleServiceModel>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Display(Name = "Link to the image you'd like to use for your article")]
        public string ImageUrl { get; set; }

        // TODO: Custom validation on front & backend checking whether we have one or the other - Picture or imageUrl
        public IFormFile Image { get; set; }
    }
}
