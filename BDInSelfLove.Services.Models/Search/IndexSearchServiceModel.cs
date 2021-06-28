using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Services.Models.Video;
using BDInSelfLove.Services.Models.Videos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Search
{
    public class IndexSearchServiceModel
    {
        public List<VideoPreviewServiceModel> Videos { get; set; }

        public int VideosCount { get; set; }

        public List<ArticlePreviewServiceModel> Articles { get; set; }

        public int ArticlesCount { get; set; }
    }
}
