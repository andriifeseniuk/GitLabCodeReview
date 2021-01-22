using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProjectDto
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
