using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.ArticleComment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.ArticleComment
{
    public class ArticleCommentService : IArticleCommentService
    {
        private const int MaxCommentNestingDepth = 3;
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.ArticleComment> articleCommentRepository;

        public ArticleCommentService(IDeletableEntityRepository<BDInSelfLove.Data.Models.ArticleComment> articleCommentRepository)
        {
            this.articleCommentRepository = articleCommentRepository;
        }

        public async Task<int> Create(ArticleCommentServiceModel articleCommentServiceModel)
        {
            var parentCommentId = articleCommentServiceModel.ParentCommentId;
            if (parentCommentId != null && await this.CheckCommentDepth(parentCommentId) >= MaxCommentNestingDepth)
            {
                // Set upper level parent to avoid too much nesting
                articleCommentServiceModel.ParentCommentId = (await this.articleCommentRepository.All()
                    .SingleOrDefaultAsync(c => c.Id == parentCommentId)).ParentCommentId;
            }

            var articleComment = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.ArticleComment>(articleCommentServiceModel);

            await this.articleCommentRepository.AddAsync(articleComment);
            await this.articleCommentRepository.SaveChangesAsync();
            return articleComment.Id;
        }

        public IQueryable<ArticleCommentServiceModel> GetAllByArticleId(int articleId)
        {
            return this.articleCommentRepository.All()
                .Where(a => a.ArticleId == articleId)
                .To<ArticleCommentServiceModel>();
        }

        public IQueryable<ArticleCommentServiceModel> GetById(int commentId)
        {
            return this.articleCommentRepository.All()
                .Where(a => a.Id == commentId)
                .To<ArticleCommentServiceModel>();
        }

        public async Task<int> Edit(ArticleCommentServiceModel serviceModel)
        {
            var dbComment = await this.articleCommentRepository.All().SingleOrDefaultAsync(a => a.Id == serviceModel.Id);

            if (dbComment == null)
            {
                return 0;
            }

            dbComment.Content = serviceModel.Content;

            this.articleCommentRepository.Update(dbComment);
            int result = await this.articleCommentRepository.SaveChangesAsync();

            return result;
        }

        private async Task<int> CheckCommentDepth(int? parentCommentId, int depthLevel = 1)
        {
            var parentComment = await this.articleCommentRepository.All().Where(c => c.Id == parentCommentId).FirstOrDefaultAsync();
            if (parentComment.ParentCommentId != null)
            {
                depthLevel++;
                depthLevel = await this.CheckCommentDepth(parentComment.ParentCommentId, depthLevel);
            }

            return depthLevel;
        }
    }
}
