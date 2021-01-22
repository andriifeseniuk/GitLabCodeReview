using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NoteDto
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "author")]
        public GitLabUser Author { get; set; }

        [JsonProperty(PropertyName = "position")]
        public PositionDto Position { get; set; }
    }
}
