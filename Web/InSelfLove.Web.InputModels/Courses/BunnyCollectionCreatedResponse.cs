using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InSelfLove.Web.InputModels.Courses
{
    public class BunnyCollectionCreatedResponse
    {
        [JsonPropertyName("guid")]
        public string Guid { get; init; }
    }
}
