using System.Collections.Generic;

namespace GitLabCodeReview.Models
{
    public class Hunk
    {
        public int StartInSourceFile { get; set; }
        public int LengthInSourceFile { get; set; }
        public int EndInSourceFile => this.StartInSourceFile + this.LengthInSourceFile - 1;

        public int StartInTargetFile { get; set; }
        public int LengthInTargetFile { get; set; }
        public int EndInTargetFile => this.StartInTargetFile + this.LengthInTargetFile - 1;

        public int StartInChanges { get; set; }
        public int LengthInChanges => this.Lines.Count;
        public int EndInChanges => this.StartInChanges + this.LengthInChanges - 1;

        public List<HunkLine> Lines { get; set; } = new List<HunkLine>();
    }
}
