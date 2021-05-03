namespace GitLabCodeReview.Models
{
    public class HunkLine
    {
        public int? NumberInSourceFile { get; set; }
        public int? NumberInTargetFile { get; set; }
        public bool IsLineAdded => this.NumberInSourceFile != null && this.NumberInTargetFile == null;
        public bool IsLineRemoved => this.NumberInSourceFile == null && this.NumberInTargetFile != null;
        public string Text { get; set; }
    }
}
