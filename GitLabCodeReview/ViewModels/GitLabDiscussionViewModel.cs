using GitLabCodeReview.Models;
using System.Linq;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabDiscussionViewModel : BaseViewModel
    {
        public GitLabDiscussionViewModel(GitLabDiscussion gitLabDiscussion)
        {
            this.Discussion = gitLabDiscussion;
        }

        public GitLabDiscussion Discussion { get; }

        public string Title => this.GetTitle();

        private string GetTitle()
        {
            var position = this.Discussion.Notes.First().Position;
            var lineLabel = position.NewLine != null
                ? $"+{position.NewLine.Value}"
                : $"-{position.OldLine.Value}";

            return $"{lineLabel}: TODO";
        }
    }
}
