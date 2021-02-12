using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Services.Models.Videos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Search
{
    public class IndexSearchServiceModel
    {
        public List<VideoServiceModel> Videos { get; set; }

        public int VideosCount { get; set; }

        public List<ArticleServiceModel> Articles { get; set; }

        public int ArticlesCount { get; set; }
    }
}
