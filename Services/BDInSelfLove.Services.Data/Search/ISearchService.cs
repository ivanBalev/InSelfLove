using BDInSelfLove.Services.Models.Search;
using BDInSelfLove.Services.Models.Videos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Search
{
    public interface ISearchService
    {
        Task<IndexSearchServiceModel> Index(string searchTerm);

        Task<VideosSearchServiceModel> GetVideos(string searchTerm, int take, int skip = 0);

        Task<ArticlesSearchServiceModel> GetArticles(string searchTerm, int take, int skip = 0);
    }
}
