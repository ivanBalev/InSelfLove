using BDInSelfLove.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Text;


namespace BDInSelfLove.Services.Models.Video
{
    public class VideoPreviewServiceModel : IMapFrom<Data.Models.Video>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
