using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabDiscussion
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public GitLabNote[] Notes { get; set; }
    }
}
