namespace BDInSelfLove.Web.ViewModels.Appointment
{
    using System;

    using AutoMapper;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using Newtonsoft.Json;

    public class AppointmentViewModel : IMapFrom<Appointment>, IHaveCustomMappings
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("title")]
        public string Title => this.Start.ToString("HH:mm");

        [JsonProperty("isApproved")]
        public bool IsApproved { get; set; }

        [JsonProperty("isUnavailable")]
        public bool IsUnavailable { get; set; }

        [JsonProperty("isOnSite")]
        public bool IsOnSite { get; set; }

        [JsonProperty("canBeOnSite")]
        public bool CanBeOnSite { get; set; }

        [JsonProperty("userName")]
        public string UserUserName { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Appointment, AppointmentViewModel>().ForMember(
                m => m.Start,
                opt => opt.MapFrom(x => x.UtcStart));
        }
    }
}
