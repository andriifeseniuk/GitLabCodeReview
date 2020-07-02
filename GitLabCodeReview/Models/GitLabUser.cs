using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabUser
    {
        [JsonProperty(PropertyName = "id")]
        public long UserId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }
    }
}
