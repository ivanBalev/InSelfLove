namespace BDInSelfLove.Services.Data.Comments
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using Microsoft.EntityFrameworkCore;

    public class CommentService : ICommentService
    {
        private readonly IDeletableEntityRepository<Comment> commentRepository;

        public CommentService(IDeletableEntityRepository<Comment> commentRepository)
        {
            this.commentRepository = commentRepository;
        }

        public async Task<int> Create(Comment comment)
        {
            await this.SetCommentDepth(comment);
            await this.commentRepository.AddAsync(comment);
            await this.commentRepository.SaveChangesAsync();
            return comment.Id;
        }

        public IQueryable<Comment> GetAllByArticleId(int articleId)
        {
            return this.commentRepository.All()
                .Where(c => c.ArticleId == articleId);
        }

        public IQueryable<Comment> GetAllByVideoId(int videoId)
        {
            return this.commentRepository.All()
                .Where(c => c.VideoId == videoId);
        }

        public IQueryable<Comment> GetById(int commentId)
        {
            return this.commentRepository.All()
                .Where(a => a.Id == commentId);
        }

        public async Task<int> Edit(Comment comment, string userId)
        {
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

        private async Task SetCommentDepth(Comment comment)
        {
            var parentCommentId = comment.ParentCommentId;

            if (parentCommentId != null)
            {
                var grandParentCommentId = (await this.commentRepository.All()
                    .FirstOrDefaultAsync(c => c.Id == parentCommentId)).ParentCommentId;

                if (grandParentCommentId != null)
                {
                    var greatGrandParentCommentId = (await this.commentRepository.All()
                    .FirstOrDefaultAsync(c => c.Id == grandParentCommentId)).ParentCommentId;

                    if (greatGrandParentCommentId != null)
                    {
                        // Set parent 1 level up to avoid too much nesting
                        comment.ParentCommentId = grandParentCommentId;
                    }
                }
            }
        }
    }
}
