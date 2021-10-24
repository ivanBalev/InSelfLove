
using System.Linq;

namespace BDInSelfLove.Services.Models.Search
{
    public class IndexSearchServiceModel
    {
        public IQueryable<Data.Models.Video> Videos { get; set; }

        public int VideosCount { get; set; }

        public IQueryable<Data.Models.Article> Articles { get; set; }

        public int ArticlesCount { get; set; }
    }
}
