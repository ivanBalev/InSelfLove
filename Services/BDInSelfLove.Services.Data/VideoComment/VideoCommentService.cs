using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.VideoComment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.VideoComment
{
    public class VideoCommentService : IVideoCommentService
    {
        private const int MaxCommentNestingDepth = 3;
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.VideoComment> videoCommentRepository;

        public VideoCommentService(IDeletableEntityRepository<BDInSelfLove.Data.Models.VideoComment> videoCommentRepository)
        {
            this.videoCommentRepository = videoCommentRepository;
        }

        public async Task<int> Create(VideoCommentServiceModel videoCommentServiceModel)
        {
            var parentCommentId = videoCommentServiceModel.ParentCommentId;
            if (parentCommentId != null && await this.CheckCommentDepth(parentCommentId) >= MaxCommentNestingDepth)
            {
                // Set upper level parent to avoid too much nesting
                videoCommentServiceModel.ParentCommentId = (await this.videoCommentRepository.All()
                    .SingleOrDefaultAsync(c => c.Id == parentCommentId)).ParentCommentId;
            }

            var videoComment = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.VideoComment>(videoCommentServiceModel);

            await this.videoCommentRepository.AddAsync(videoComment);
            await this.videoCommentRepository.SaveChangesAsync();
            return videoComment.Id;
        }

        public IQueryable<VideoCommentServiceModel> GetAllByVideoId(int videoId)
        {
            return this.videoCommentRepository.All()
                .Where(vc => vc.VideoId == videoId)
                .To<VideoCommentServiceModel>();
        }

        private async Task<int> CheckCommentDepth(int? parentCommentId, int depthLevel = 1)
        {
            var parentComment = await this.videoCommentRepository.All().Where(c => c.Id == parentCommentId).FirstOrDefaultAsync();
            if (parentComment.ParentCommentId != null)
            {
                depthLevel++;
                depthLevel = await this.CheckCommentDepth(parentComment.ParentCommentId, depthLevel);
            }

            return depthLevel;
        }
    }
}
