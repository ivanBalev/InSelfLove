using BDInSelfLove.Web.ViewModels.Article;
using BDInSelfLove.Web.ViewModels.Home;
using BDInSelfLove.Web.ViewModels.Video;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Search
{
    public class IndexSearchViewModel
    {
        public string SearchTerm { get; set; }

        public VideoPaginationViewModel VideosPagination { get; set; }

        public ArticlesPaginationViewModel ArticlesPagination { get; set; }
    }
}
