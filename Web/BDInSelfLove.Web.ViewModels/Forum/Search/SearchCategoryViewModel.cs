namespace BDInSelfLove.Web.ViewModels.Forum.Search
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;

    public class SearchCategoryViewModel : IMapFrom<CategoryServiceModel>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int PostsCount { get; set; }

        public int CommentsCount { get; set; }
    }
}
