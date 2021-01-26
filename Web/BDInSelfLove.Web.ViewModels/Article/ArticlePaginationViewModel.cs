using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Article;
using BDInSelfLove.Web.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Article
{
    public class ArticlePaginationViewModel
    {
        public ICollection<BriefArticleInfoViewModel> Articles { get; set; }

        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }
    }
}
