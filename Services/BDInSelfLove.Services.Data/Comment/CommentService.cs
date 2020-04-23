namespace BDInSelfLove.Services.Data.Comment
{
    using System.Collections;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;
    using Microsoft.EntityFrameworkCore;

    public class CommentService : ICommentService
    {
        private readonly IDeletableEntityRepository<Comment> commentRepository;
        private readonly IDeletableEntityRepository<Report> reportRepository;

        public CommentService(IDeletableEntityRepository<Comment> commentRepository, IDeletableEntityRepository<Report> reportRepository)
        {
            this.commentRepository = commentRepository;
            this.reportRepository = reportRepository;
        }


        public int CommentsCountByCategoryId(int categoryId)
        {
            return this.commentRepository.All().Where(c => c.ParentPost.CategoryId == categoryId).Count();
        }

        public async Task<int> Create(CommentServiceModel categoryServiceModel)
        {
            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(categoryServiceModel);

            await this.commentRepository.AddAsync(comment);
            await this.commentRepository.SaveChangesAsync();

            return comment.Id;
        }

        public IQueryable<CommentServiceModel> GetAllByUserId(string userId, int count = int.MaxValue)
        {
            var query = this.commentRepository.All().Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedOn).Take(count)
                .Select(c => new CommentServiceModel
                {
                    ParentPostId = c.ParentPostId,
                    ParentPost = new PostServiceModel {
                        Title = c.ParentPost.Title,
                    },
                    Content = c.Content,
                    CreatedOn = c.CreatedOn,
                });

            return query;
        }

        public async Task GetAllSubComments(CommentServiceModel comment, PostServiceModel post)
        {
            var subComments = post.Comments.Where(c => c.ParentCommentId == comment.Id).ToList();

            comment.SubComments = subComments;

            for (var i = 0; i < subComments.Count; i++)
            {
                await this.GetAllSubComments(subComments[i], post);
            }
        }

        public IQueryable<CommentServiceModel> GetById(int id)
        {
            // TODO: Could not get the following to work. Does not want to take ParentPost.Title 

            //var test1 = this.commentRepository.All().Where(c => c.Id == id).To<CommentServiceModel>().ToList();

            //var test2 = this.commentRepository.All().Where(c => c.Id == id).Include(c => c.ParentPost).To<CommentServiceModel>().ToList();

            return this.commentRepository.All().Where(c => c.Id == id).To<CommentServiceModel>();
        }

        public async Task<int> SubmitReport(ReportServiceModel reportService)
        {
            var report = AutoMapperConfig.MapperInstance.Map<Report>(reportService);
            report.IsApproved = false;

            await this.reportRepository.AddAsync(report);
            await this.commentRepository.SaveChangesAsync();

            return report.Id;
        }
    }
}
