using GitLabCodeReview.DTO;
using System.Linq;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabDiscussionViewModel : BaseViewModel
    {
        private string[] sourceFileLines;
        private string[] targetFileLines;

        public GitLabDiscussionViewModel(GitLabDiscussion gitLabDiscussion, string[] sourceFileLines, string[] targetFileLines)
        {
            this.Discussion = gitLabDiscussion;
            this.sourceFileLines = sourceFileLines;
            this.targetFileLines = targetFileLines;
        }

        public GitLabDiscussion Discussion { get; }

        public string Title => this.GetTitle();

        private string GetTitle()
        {
            var position = this.Discussion.Notes.First().Position;
            if (position.NewLine != null)
            {
                return $"+{position.NewLine.Value}: {sourceFileLines[position.NewLine.Value - 1]}";
            }
            else
            {
                return $"-{position.OldLine.Value}: {targetFileLines[position.OldLine.Value - 1]}";
            }
        }
    }
}
