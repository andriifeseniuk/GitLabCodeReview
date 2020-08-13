using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabNote
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "author")]
        public GitLabUser Name { get; set; }
    }
}
