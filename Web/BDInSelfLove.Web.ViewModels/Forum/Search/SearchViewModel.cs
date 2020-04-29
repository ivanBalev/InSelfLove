namespace BDInSelfLove.Web.ViewModels.Forum.Search
{
    using System.Collections.Generic;

    public class SearchViewModel
    {
        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPostsCount { get; set; }

        public int TotalCategoriesCount { get; set; }

        public int TotalCommentsCount { get; set; }

        public List<SearchPostViewModel> Posts { get; set; }

        public List<SearchCategoryViewModel> Categories { get; set; }

        public List<SearchCommentViewModel> Comments { get; set; }

        public string SearchTerm { get; set; }

        public string UserId { get; set; }
    }
}
