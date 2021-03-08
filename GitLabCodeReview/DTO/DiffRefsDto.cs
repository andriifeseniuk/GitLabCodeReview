using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DiffRefsDto
    {
        [JsonProperty(PropertyName = "base_sha")]
        public string BaseSha { get; set; }

        [JsonProperty(PropertyName = "start_sha")]
        public string StartSha { get; set; }

        [JsonProperty(PropertyName = "head_sha")]
        public string HeadSha { get; set; }
    }
}
