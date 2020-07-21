using GitLabCodeReview.Models;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabChangeViewModel
    {
        private GitLabChange change;

        public GitLabChangeViewModel(GitLabChange gitLabChange)
        {
            this.change = gitLabChange;
        }

        public string FileDisplayName
        {
            get
            {
                if (this.change.IsNewFile)
                {
                    return this.change.NewPath;
                }

                if (this.change.IsDeletedFile)
                {
                    return this.change.OldPath;
                }

                if (this.change.IsRenamedFile)
                {
                    return $"{this.change.NewPath}[{this.change.OldPath}]";
                }

                return this.change.NewPath;
            }
        }
    }
}
