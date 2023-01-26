namespace InSelfLove.Services.Data.Comments
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using Microsoft.EntityFrameworkCore;

    public class CommentService : ICommentService
    {
        private readonly IDeletableEntityRepository<Comment> commentRepository;

        public CommentService(IDeletableEntityRepository<Comment> commentRepository)
        {
            this.commentRepository = commentRepository;
        }

        public async Task<int> Create(Comment comment, string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrEmpty(comment.Content) || string.IsNullOrWhiteSpace(comment.Content) ||
                (comment.ArticleId == null && comment.VideoId == null))
            {
                throw new ArgumentException(nameof(comment));
            }

            comment.UserId = userId;

            // Avoid deep nesting (2 levels max)
            await this.SetCommentDepth(comment);

            // Save to db and return Id
            await this.commentRepository.AddAsync(comment);
            await this.commentRepository.SaveChangesAsync();
            return comment.Id;
        }

        public IQueryable<Comment> GetById(int commentId)
        {
            return this.commentRepository.All()
                .Where(a => a.Id == commentId);
        }

        public async Task<int> Edit(Comment comment, string userId)
        {
            if (comment == null || string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
            {
                return 0;
            }

            var dbComment = await this.commentRepository.All().Where(c => c.Id == comment.Id).SingleOrDefaultAsync();

            // Check if comment exists & creator is same as editor
            if (dbComment == null || dbComment.UserId != userId)
            {
                return 0;
            }

            dbComment.Content = comment.Content;
            this.commentRepository.Update(dbComment);

            return await this.commentRepository.SaveChangesAsync();
        }

        public async Task<int> Delete(int commentId, string userId, bool isUserAdmin)
        {
            var comment = await this.commentRepository.All().SingleOrDefaultAsync(c => c.Id == commentId);

            if (comment == null || (comment.UserId != userId && !isUserAdmin))
            {
                // Comment doesn't exist || user not same as creator and not admin
                return 0;
            }

            var secondLevelComments = await this.commentRepository.All().Where(c => c.ParentCommentId == commentId).ToListAsync();

            foreach (var nestedComment in secondLevelComments)
            {
                // Delete most deeply nested comments
                var thirdLevelComments = await this.commentRepository.All().Where(c => c.ParentCommentId == nestedComment.Id).ToListAsync();
                foreach (var lastLevelComment in thirdLevelComments)
                {
                    this.commentRepository.Delete(lastLevelComment);
                }

                // Delete second level comments
                this.commentRepository.Delete(nestedComment);
            }

            // Delete comment
            this.commentRepository.Delete(comment);
            int result = await this.commentRepository.SaveChangesAsync();
            return result;
        }

        public ICollection<Comment> ArrangeCommentHierarchy(ICollection<Comment> comments)
        {
            // Populate subcomments references

            // Get all Subcomments
            foreach (var comment in comments.Where(c => c.ParentCommentId != null))
            {
                // Find the subcomment's parent
                var parentComment = comments.SingleOrDefault(x => x.Id == comment.ParentCommentId);

                // Add the subcomment to the parent's subcomments list
                parentComment.SubComments.Add(comment);
            }

            // Remove subcomments from main structure
            // Leave only nested structure that we just created
            // And order main comments by date for client
            comments = comments.Where(c => c.ParentCommentId == null)
                               .OrderByDescending(c => c.CreatedOn.Date)
                               .ThenByDescending(c => c.CreatedOn.TimeOfDay).ToList();

            // Order subcomments
            foreach (var mainComment in comments)
            {
                // Order 1st level subcomments
                mainComment.SubComments = mainComment.SubComments?
                               .OrderByDescending(c => c.CreatedOn.Date)
                               .ThenByDescending(c => c.CreatedOn.TimeOfDay).ToList();

                foreach (var subComment in mainComment.SubComments)
                {
                    // Order 2nd level subcomments (max level of nesting is 2nd level by default)
                    subComment.SubComments = subComment.SubComments?
                               .OrderByDescending(c => c.CreatedOn.Date)
                               .ThenByDescending(c => c.CreatedOn.TimeOfDay).ToList();
                }
            }

            return comments;
        }

        // If comment is more than 2 levels nested, set it to default max nesting of 2nd level
        public async Task SetCommentDepth(Comment comment)
        {
            var parentCommentId = comment.ParentCommentId;

            if (parentCommentId != null)
            {
                var grandParentCommentId = (await this.commentRepository.All()
                    .FirstOrDefaultAsync(c => c.Id == parentCommentId))?.ParentCommentId;

                if (grandParentCommentId != null)
                {
                    var greatGrandParentCommentId = (await this.commentRepository.All()
                    .FirstOrDefaultAsync(c => c.Id == grandParentCommentId))?.ParentCommentId;

                    if (greatGrandParentCommentId != null)
                    {
                        // Comment is 3 levels deep nested
                        // Set parent 1 level up to avoid too much nesting
                        comment.ParentCommentId = grandParentCommentId;
                    }
                }
            }
        }
    }
}
