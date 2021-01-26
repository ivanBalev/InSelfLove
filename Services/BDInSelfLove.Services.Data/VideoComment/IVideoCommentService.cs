using BDInSelfLove.Services.Models.VideoComment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.VideoComment
{
    public interface IVideoCommentService
    {
        Task<int> Create(VideoCommentServiceModel videoCommentServiceModel);

        IQueryable<VideoCommentServiceModel> GetAllByVideoId(int videoId);
    }
}
