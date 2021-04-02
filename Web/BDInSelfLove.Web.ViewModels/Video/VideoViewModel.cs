using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Videos;
using BDInSelfLove.Web.ViewModels.VideoComment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Video
{
    public class VideoViewModel : IMapFrom<VideoServiceModel>
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public ICollection<VideoCommentViewModel> VideoComments { get; set; }

        public string Title { get; set; }
    }
}
