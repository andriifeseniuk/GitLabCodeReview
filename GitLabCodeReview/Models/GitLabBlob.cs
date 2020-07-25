using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabBlob
    {
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
