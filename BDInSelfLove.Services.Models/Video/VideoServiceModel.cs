namespace BDInSelfLove.Services.Models.Videos
{
    using System;
    using System.Collections.Generic;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Services.Models.User;

    public class VideoServiceModel : IMapTo<Video>, IMapFrom<Video>
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public DateTime ModifiedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }

        public ICollection<CommentServiceModel> Comments { get; set; }

        public string Title { get; set; }

        public string AssociatedTerms { get; set; }
    }
}
