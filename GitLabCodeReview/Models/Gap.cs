namespace GitLabCodeReview.Models
{
    public class Gap
    {
        public int StartInSourceFile { get; set; }
        public int EndInSourceFile => this.StartInSourceFile + this.Length - 1;

        public int StartInTargetFile { get; set; }
        public int EndInTargetFile => this.StartInTargetFile + this.Length - 1;

        public int StartInChanges { get; set; }
        public int EndInChanges => this.StartInChanges + this.Length - 1;

        public int Length { get; set; }
    }
}
