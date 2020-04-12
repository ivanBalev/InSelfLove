using BDInSelfLove.Services.Mapping;
using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Search
{
    public class SearchViewModel
    {
        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }

        public List<SearchPostViewModel> Posts { get; set; }

        public string SearchTerm { get; set; }

        public string UserId { get; set; }
    }
}
