namespace BDInSelfLove.Services.Models.Videos
{
    using System;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.User;

    public class VideoServiceModel : IMapTo<Video>, IMapFrom<Video>
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }
    }
}
