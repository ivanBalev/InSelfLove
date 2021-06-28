using BDInSelfLove.Services.Models.Video;
using System.Collections.Generic;

namespace BDInSelfLove.Services.Models.Search
{
    public class VideosSearchServiceModel
    {
        public ICollection<VideoPreviewServiceModel> Videos { get; set; }

        public int VideosCount { get; set; }
    }
}
