using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FileDto
    {
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "blob_id")]
        public string BlobId { get; set; }

        [JsonProperty(PropertyName = "encoding")]
        public string Encoding { get; set; }
    }
}
