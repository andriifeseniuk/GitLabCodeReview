using GitLabCodeReview.Models;

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
            return "+1: TODO";
        }
    }
}
