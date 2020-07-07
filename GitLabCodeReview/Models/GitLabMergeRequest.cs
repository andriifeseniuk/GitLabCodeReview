﻿using Newtonsoft.Json;

namespace GitLabCodeReview.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GitLabMergeRequest
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "iid")]
        public long InternalId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }
}