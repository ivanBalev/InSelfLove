namespace BDInSelfLove.Web.ViewModels.Forum.Category
{
    using System.Collections.Generic;
    using BDInSelfLove.Services.Mapping;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class CategorySortingModel
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
