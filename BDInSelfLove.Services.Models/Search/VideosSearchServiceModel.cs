using BDInSelfLove.Services.Models.Videos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Search
{
    public class VideosSearchServiceModel
    {
        public ICollection<VideoServiceModel> Videos { get; set; }

        public int VideosCount { get; set; }
    }
}
