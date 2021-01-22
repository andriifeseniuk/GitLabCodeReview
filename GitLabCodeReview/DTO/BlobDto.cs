using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BlobDto
    {
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
