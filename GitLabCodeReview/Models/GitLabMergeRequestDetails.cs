using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabMergeRequestDetails
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "iid")]
        public long InternalId { get; set; }

        [JsonProperty(PropertyName = "changes")]
        public GitLabChange[] Changes { get; set; }
    }
}
