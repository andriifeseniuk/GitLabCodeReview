using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.Models;
using System.Windows.Input;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabChangeViewModel
    {
        private GitLabChange change;
        private MainViewModel mainViewModel;

        public GitLabChangeViewModel(GitLabChange gitLabChange, MainViewModel mainVM)
        {
            this.change = gitLabChange;
            this.mainViewModel = mainVM;
            this.DiffCommand = new DelegateCommand(x => this.ExecuteDiff());
        }

        public ICommand DiffCommand { get; } 

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

        private void ExecuteDiff()
        {
            this.mainViewModel.ExecuteDiff(this.change);
        }
    }
}
