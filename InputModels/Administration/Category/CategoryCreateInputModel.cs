using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Category;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Administration.Category
{
    public class CategoryCreateInputModel : IMapTo<CategoryServiceModel>
    {
        [Required]
        [MinLength(15)]
        public string Name { get; set; }

        [Required]
        [MinLength(20)]
        public string Description { get; set; }
    }
}
