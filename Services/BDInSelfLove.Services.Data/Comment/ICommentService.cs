namespace BDInSelfLove.Services.Data.Comment
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.Post;

    public interface ICommentService
    {
        Task<int> Create(CommentServiceModel categoryServiceModel);

        Task GetAllSubComments(CommentServiceModel comment, PostServiceModel post);

        IQueryable<CommentServiceModel> GetAllByUserId(string userId, int count = int.MaxValue);

        IQueryable<CommentServiceModel> GetById(int id);

        Task<int> SubmitReport(ReportServiceModel reportService);

        int CommentsCountByCategoryId(int categoryId);

        IQueryable<CommentServiceModel> GetCommentWithReport(int reportId);

        Task<int> AddReportAssessment(int reportId, bool assessment, string offenderId);

        Task ClearUserReports(string userId);

        IQueryable<CommentServiceModel> Search(string searchTerm);
    }
}
