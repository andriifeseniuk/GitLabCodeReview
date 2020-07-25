using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabFile
    {
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "blob_id")]
        public string BlobId { get; set; }

        [JsonProperty(PropertyName = "encoding")]
        public string Encoding { get; set; }
    }
}
