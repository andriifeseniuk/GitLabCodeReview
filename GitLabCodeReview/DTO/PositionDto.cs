using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    //[JsonObject(MemberSerialization.OptIn)]
    public class PositionDto
    {
        [JsonProperty(PropertyName = "base_sha")]
        public string BaseSha { get; set; }

        [JsonProperty(PropertyName = "start_sha")]
        public string StartSha { get; set; }

        [JsonProperty(PropertyName = "head_sha")]
        public string HeadSha { get; set; }

        [JsonProperty(PropertyName = "position_type")]
        public string PositionType { get; set; }

        [JsonProperty(PropertyName = "old_path")]
        public string OldPath { get; set; }

        [JsonProperty(PropertyName = "new_path")]
        public string NewPath { get; set; }

        [JsonProperty(PropertyName = "old_line")]
        public int? OldLine { get; set; }

        [JsonProperty(PropertyName = "new_line")]
        public int? NewLine { get; set; }
    }
}
