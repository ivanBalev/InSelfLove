using InSelfLove.Services.Mapping;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace InSelfLove.Web.InputModels.Video
{
    public class CreateVideoInputModel : IMapTo<Data.Models.Video>, IMapFrom<Data.Models.Video>
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

        public string Slug => Regex.Replace(this.Title.ToLower().Replace(' ', '-'), "[^a-zа-я0-9-_~]+", string.Empty);

        public DateTime CreatedOn { get; set; }
    }
}
