using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Forum.Category
{
    public class CategorySortingInputModel
    {
        public int CategoryId { get; set; }

        public string TimeCriterion { get; set; }

        public List<SelectListItem> TimeCriteria { get; set; }

        public string GroupingCriterion { get; set; }

        public List<SelectListItem> GroupingCriteria { get; set; }

        public string OrderingCriterion { get; set; }

        public List<SelectListItem> OrderingCriteria { get; set; }
    }
}
