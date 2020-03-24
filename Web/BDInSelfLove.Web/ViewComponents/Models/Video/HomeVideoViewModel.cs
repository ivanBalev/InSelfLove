namespace BDInSelfLove.Web.ViewComponents.Models.Video
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;

    public class HomeVideoViewModel : IMapFrom<VideoServiceModel>
    {
        public int Id { get; set; }

        public string Url { get; set; }
    }
}
