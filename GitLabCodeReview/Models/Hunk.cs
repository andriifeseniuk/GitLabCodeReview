using System.Collections.Generic;

namespace GitLabCodeReview.Models
{
    public class Hunk
    {
        public int StartInSourceFile { get; set; }
        public int LengthInSourceFile { get; set; }
        public int StartInTargetFile { get; set; }
        public int LenghtInTargetFile { get; set; }
        public List<HunkLine> Lines { get; set; } = new List<HunkLine>();
    }
}
