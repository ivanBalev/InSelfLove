using BDInSelfLove.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Article
{
    public class ArticlePreviewServiceModel : IMapFrom<Data.Models.Article>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }
    }
}
