using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Forum.Category
{
    public class CategorySortingInputModel
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string TimeCriterion { get; set; }

        public List<SelectListItem> TimeCriteria { get; set; }

        [Required]
        public string GroupingCriterion { get; set; }

        public List<SelectListItem> GroupingCriteria { get; set; }

        [Required]
        public string OrderingCriterion { get; set; }

        public List<SelectListItem> OrderingCriteria { get; set; }
    }
}
