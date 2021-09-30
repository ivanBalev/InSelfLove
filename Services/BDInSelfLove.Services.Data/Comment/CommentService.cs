using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.CommentService
{
    public class CommentService : ICommentService
    {
        private const int MaxCommentNestingDepth = 3;
        private readonly IDeletableEntityRepository<Comment> commentRepository;

        public CommentService(IDeletableEntityRepository<Comment> commentRepository)
        {
            this.commentRepository = commentRepository;
        }

        public async Task<int> Create(CommentServiceModel commentServiceModel)
        {
            var parentCommentId = commentServiceModel.ParentCommentId;
            if (parentCommentId != null && await this.CheckCommentDepth(parentCommentId) >= MaxCommentNestingDepth)
            {
                // Set upper level parent to avoid too much nesting
                commentServiceModel.ParentCommentId = (await this.commentRepository.All()
                    .SingleOrDefaultAsync(c => c.Id == parentCommentId)).ParentCommentId;
            }

            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(commentServiceModel);

            await this.commentRepository.AddAsync(comment);
            await this.commentRepository.SaveChangesAsync();
            return comment.Id;
        }

        public IQueryable<CommentServiceModel> GetAllByArticleId(int articleId)
        {
            return this.commentRepository.All()
                .Where(a => a.ArticleId == articleId)
                .To<CommentServiceModel>();
        }

        public IQueryable<CommentServiceModel> GetAllByVideoId(int videoId)
        {
            return this.commentRepository.All()
                .Where(vc => vc.VideoId == videoId)
                .To<CommentServiceModel>();
        }

        public IQueryable<CommentServiceModel> GetById(int commentId)
        {
            return this.commentRepository.All()
                .Where(a => a.Id == commentId)
                .To<CommentServiceModel>();
        }

        public async Task<int> Edit(CommentServiceModel serviceModel)
        {
            var dbComment = await this.commentRepository.All().SingleOrDefaultAsync(a => a.Id == serviceModel.Id);

            if (dbComment == null)
            {
                return 0;
            }

            dbComment.Content = serviceModel.Content;

            this.commentRepository.Update(dbComment);
            int result = await this.commentRepository.SaveChangesAsync();

            return result;
        }

        public async Task<int> Delete(int commentId)
        {
            var secondLevelComments = await this.commentRepository.All().Where(c => c.ParentCommentId == commentId).ToListAsync();

            foreach (var comment in secondLevelComments)
            {
                // Delete most deeply nested comments
                var thirdLevelComments = await this.commentRepository.All().Where(c => c.ParentCommentId == comment.Id).ToListAsync();
                foreach (var lastLevelComment in thirdLevelComments)
                {
                    this.commentRepository.Delete(lastLevelComment);
                }

                // Delete second level comments
                this.commentRepository.Delete(comment);
            }

            // Delete selected comment
            var selectedComment = await this.commentRepository.All().SingleOrDefaultAsync(c => c.Id == commentId);

            // Check whether invalid comment id was entered manually by user
            if (selectedComment == null)
            {
                return 0;
            }

            this.commentRepository.Delete(selectedComment);
            int result = await this.commentRepository.SaveChangesAsync();

            return result;
        }

        public ICollection<CommentServiceModel> ArrangeCommentHierarchy(ICollection<CommentServiceModel> comments)
        {
            // Clear ef subcomment structure as it only goes 1 level deep
            foreach (var comment in comments)
            {
                comment.SubComments.Clear();
            }

            // Populate subcomments references
            foreach (var comment in comments.Where(c => c.ParentCommentId != null))
            {
                var parentComment = comments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                parentComment.SubComments.Add(comment);
            }

            // Remove subcomments from main structure, leaving only nested structure, and order comments
            comments = comments.Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedOn).ToList();

            // Order subcomments
            foreach (var comment in comments)
            {
                comment.SubComments = comment.SubComments?.OrderByDescending(c => c.CreatedOn).ToList();

                foreach (var subcomment in comment.SubComments)
                {
                    subcomment.SubComments = subcomment.SubComments?.OrderByDescending(c => c.CreatedOn).ToList();
                }
            }

            return comments;
        }

        private async Task<int> CheckCommentDepth(int? parentCommentId, int depthLevel = 1)
        {
            var parentComment = await this.commentRepository.All().Where(c => c.Id == parentCommentId).FirstOrDefaultAsync();
            if (parentComment.ParentCommentId != null)
            {
                depthLevel++;
                depthLevel = await this.CheckCommentDepth(parentComment.ParentCommentId, depthLevel);
            }

            return depthLevel;
        }
    }
}
