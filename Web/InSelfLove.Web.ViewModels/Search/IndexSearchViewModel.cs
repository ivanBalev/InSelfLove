using InSelfLove.Web.ViewModels.Article;
using InSelfLove.Web.ViewModels.Home;
using InSelfLove.Web.ViewModels.Video;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSelfLove.Web.ViewModels.Search
{
    public class IndexSearchViewModel
    {
        public string SearchTerm { get; set; }

        public VideosPaginationViewModel VideosPagination { get; set; }

        public ArticlesPaginationViewModel ArticlesPagination { get; set; }
    }
}
