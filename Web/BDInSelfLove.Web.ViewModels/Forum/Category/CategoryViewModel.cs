namespace BDInSelfLove.Web.ViewModels.Forum.Category
{
    using System.Collections.Generic;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Web.InputModels.Forum.Category;

    public class CategoryViewModel : IMapFrom<CategoryServiceModel>
    {
        public CategoryViewModel()
        {
            this.Posts = new List<CategoryPostViewModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public ICollection<CategoryPostViewModel> Posts { get; set; }

        public CategorySortingInputModel CategorySorting { get; set; }
    }
}
