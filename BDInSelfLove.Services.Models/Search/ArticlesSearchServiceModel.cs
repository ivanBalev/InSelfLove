using BDInSelfLove.Services.Models.Article;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Search
{
    public class ArticlesSearchServiceModel
    {
        public ICollection<ArticlePreviewServiceModel> Articles { get; set; }

        public int ArticlesCount { get; set; }
    }
}
