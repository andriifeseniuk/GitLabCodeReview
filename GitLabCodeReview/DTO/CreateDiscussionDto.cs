using Newtonsoft.Json;

namespace GitLabCodeReview.DTO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CreateDiscussionDto
    {
        [JsonProperty(PropertyName = "position")]
        public PositionDto Position { get; set; }
    }
}
