namespace BDInSelfLove.Web.ViewModels.Administration.Dashboard.Videos
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;

    public class CreateVideoInputModel : IMapTo<VideoServiceModel>
    {
        [Required]
        [Display(Name = "Link to your video")]
        public string Url { get; set; }
    }
}
