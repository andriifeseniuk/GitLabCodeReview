﻿using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MergeRequestDto
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "iid")]
        public long InternalId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "source_branch")]
        public string SourceBranch { get; set; }

        [JsonProperty(PropertyName = "target_branch")]
        public string TargetBranch { get; set; }
    }
}
