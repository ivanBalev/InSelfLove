namespace BDInSelfLove.Services.Data.Comment
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.User;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Services.Models.User;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;

    public class CommentService : ICommentService
    {
        private readonly IDeletableEntityRepository<Comment> commentRepository;
        private readonly IDeletableEntityRepository<Report> reportRepository;
        private readonly IUserService userService;

        public CommentService(
            IDeletableEntityRepository<Comment> commentRepository,
            IDeletableEntityRepository<Report> reportRepository,
            IUserService userService)
        {
            this.commentRepository = commentRepository;
            this.reportRepository = reportRepository;
            this.userService = userService;
        }

        public IQueryable<CommentServiceModel> Search(string searchTerm)
        {
            var searchItems = SearchHelpers.GetSearchItems(searchTerm);

            var comments = this.commentRepository.All();

            foreach (var item in searchItems)
            {
                comments = comments.Where(p =>
                p.Content.ToLower().Contains(item));
            }

            return comments
                .Distinct()
                .OrderByDescending(p => p.CreatedOn)
                .To<CommentServiceModel>();
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
                    ParentPost = new PostServiceModel
                    {
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
            return this.commentRepository.All().Where(c => c.Id == id).To<CommentServiceModel>();
        }

        public async Task<int> SubmitReport(ReportServiceModel reportServiceModel)
        {
            var report = AutoMapperConfig.MapperInstance.Map<Report>(reportServiceModel);

            await this.reportRepository.AddAsync(report);
            await this.commentRepository.SaveChangesAsync();

            return report.Id;
        }

        public IQueryable<CommentServiceModel> GetCommentWithReport(int reportId)
        {
            var comment = this.commentRepository.All()
                .Where(c => c.Reports.Any(r => r.Id == reportId))
                .Select(c => new CommentServiceModel
                {
                    ParentPostId = c.ParentPostId,
                    Content = c.Content,
                    UserId = c.UserId,
                    User = new ApplicationUserServiceModel
                    {
                        UserName = c.User.UserName,
                        ProfilePhoto = c.User.ProfilePhoto,
                    },
                    Report = c.Reports.Select(r => new ReportServiceModel
                    {
                        Id = r.Id,
                        Reason = r.Reason,
                        Submitter = new ApplicationUserServiceModel
                        {
                            UserName = r.Submitter.UserName,
                            ProfilePhoto = r.Submitter.ProfilePhoto,
                        },
                    })
                    .FirstOrDefault(r => r.Id == reportId),
                });

            return comment;
        }

        public async Task<int> AddReportAssessment(int reportId, bool assessment, string offenderId)
        {
            if (assessment == true)
            {
               var reportsCount = await this.reportRepository.All()
                    .Where(r => r.IsApproved == true && r.OffenderId == offenderId)
                    .CountAsync();
               await this.userService.CheckIfUserNeedsToBeBanned(offenderId, reportsCount);
            }

            var reportFromDb = await this.reportRepository.All().FirstOrDefaultAsync(r => r.Id == reportId);
            reportFromDb.IsApproved = assessment;
            this.reportRepository.Update(reportFromDb);
            var result = await this.reportRepository.SaveChangesAsync();
            return result;
        }

        public async Task ClearUserReports(string userId)
        {
            var reports = await this.reportRepository.All()
                .Where(r => r.OffenderId == userId)
                .ToListAsync();

            reports.ForEach(r => this.reportRepository.Delete(r));
            await this.reportRepository.SaveChangesAsync();
        }
    }
}
