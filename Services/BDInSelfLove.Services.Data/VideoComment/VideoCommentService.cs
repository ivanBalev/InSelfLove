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

        public IQueryable<VideoCommentServiceModel> GetById(int commentId)
        {
            return this.videoCommentRepository.All()
                .Where(a => a.Id == commentId)
                .To<VideoCommentServiceModel>();
        }

        public async Task<int> Edit(VideoCommentServiceModel serviceModel)
        {
            var dbComment = await this.videoCommentRepository.All().SingleOrDefaultAsync(a => a.Id == serviceModel.Id);

            if (dbComment == null)
            {
                return 0;
            }

            dbComment.Content = serviceModel.Content;

            this.videoCommentRepository.Update(dbComment);
            int result = await this.videoCommentRepository.SaveChangesAsync();

            return result;
        }

        public async Task<int> Delete(int commentId)
        {
            var secondLevelComments = await this.videoCommentRepository.All().Where(c => c.ParentCommentId == commentId).ToListAsync();

            foreach (var comment in secondLevelComments)
            {
                // Delete most deeply nested comments
                var thirdLevelComments = await this.videoCommentRepository.All().Where(c => c.ParentCommentId == comment.Id).ToListAsync();
                foreach (var lastLevelComment in thirdLevelComments)
                {
                    this.videoCommentRepository.Delete(lastLevelComment);
                }

                // Delete second level comments
                this.videoCommentRepository.Delete(comment);
            }

            // Delete selected comment
            var selectedComment = await this.videoCommentRepository.All().SingleOrDefaultAsync(c => c.Id == commentId);

            // Check whether invalid comment id was entered manually by user
            if (selectedComment == null)
            {
                return 0;
            }

            this.videoCommentRepository.Delete(selectedComment);
            int result = await this.videoCommentRepository.SaveChangesAsync();

            return result;
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
