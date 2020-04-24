using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using BDInSelfLove.Services.Models.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.Comment
{
    // TODO: Implement automatic banning for a certain amount of time after let's say 5 approved reports

    public class ReportServiceModel : IMapTo<Data.Models.Report>, IMapFrom<Data.Models.Report>
    {
        public int Id { get; set; }

        public string Reason { get; set; }

        public int CommentId { get; set; }

        public CommentServiceModel Comment { get; set; }

        public string SubmitterId { get; set; }

        public ApplicationUserServiceModel Submitter { get; set; }
    }
}
