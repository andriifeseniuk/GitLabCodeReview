using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabUser
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
