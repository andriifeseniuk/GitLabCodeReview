using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabChange
    {
        [JsonProperty(PropertyName = "old_path")]
        public string OldPath { get; set; }

        [JsonProperty(PropertyName = "new_path")]
        public string NewPath { get; set; }

        [JsonProperty(PropertyName = "new_file")]
        public bool IsNewFile { get; set; }

        [JsonProperty(PropertyName = "renamed_file")]
        public bool IsRenamedFile { get; set; }

        [JsonProperty(PropertyName = "deleted_file")]
        public bool IsDeletedFile { get; set; }
    }
}
