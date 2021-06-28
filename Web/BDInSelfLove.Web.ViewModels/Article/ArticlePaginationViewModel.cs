using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Web.ViewModels.Home;
using BDInSelfLove.Web.ViewModels.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Article
{
    public class ArticlePaginationViewModel
    {
        public ICollection<ArticlePreviewViewModel> Articles { get; set; }

        public PaginationViewModel PaginationInfo { get; set; }
    }
}
