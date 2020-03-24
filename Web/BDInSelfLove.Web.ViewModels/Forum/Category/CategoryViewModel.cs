namespace BDInSelfLove.Web.ViewModels.Forum.Category
{
    using System.Collections.Generic;
    using AutoMapper;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;

    public class CategoryViewModel : IMapFrom<CategoryServiceModel>, IMapFrom<BDInSelfLove.Data.Models.Category>
    {
        public CategoryViewModel()
        {
            this.Posts = new List<PostCategoryViewModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public ICollection<PostCategoryViewModel> Posts { get; set; }
    }
}
