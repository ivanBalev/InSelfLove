using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Videos;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Video
{
    public class CreateVideoInputModel : IMapTo<VideoServiceModel>
    {
        [Required]
        [Display(Name = "Link to your video")]
        public string Url { get; set; }

        [Required]
        [Display(Name = "Its title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Key words associated with your video's content")]
        public string AssociatedTerms { get; set; }
    }
}
