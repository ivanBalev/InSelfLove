namespace BDInSelfLove.Web.ViewComponents.Models.Video
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;

    public class HomeVideoViewModel : IMapFrom<VideoServiceModel>
    {
        public string Url { get; set; }
    }
}
