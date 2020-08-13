using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabChangeViewModel : BaseViewModel
    {
        private const string MoreText = "more";
        private const string LessText = "less";
        private GitLabChange change;
        private MainViewModel mainViewModel;
        private Visibility moreSectionVisibility = Visibility.Collapsed;
        private string moreLessText = MoreText;

        public GitLabChangeViewModel(GitLabChange gitLabChange, MainViewModel mainVM)
        {
            this.change = gitLabChange;
            this.mainViewModel = mainVM;
            this.DiffCommand = new DelegateCommand(x => this.ExecuteDiff());
            this.MoreLessCommand = new DelegateCommand(x => this.ExecuteMoreLess());
        }

        public ICommand DiffCommand { get; }

        public ICommand MoreLessCommand { get; }

        public Visibility MoreSectionVisibility
        {
            get
            {
                return this.moreSectionVisibility;
            }
            set
            {
                this.moreSectionVisibility = value;
                this.SchedulePropertyChanged();
            }
        }

        public string MoreLessText
        {
            get
            {
                return this.moreLessText;
            }
            set
            {
                this.moreLessText = value;
                this.SchedulePropertyChanged();
            }
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

        public ObservableCollection<GitLabDiscussion> Discussions { get; } = new ObservableCollection<GitLabDiscussion>();

        private void ExecuteDiff()
        {
            this.mainViewModel.ExecuteDiff(this.change);
        }

        private void ExecuteMoreLess()
        {
            if (this.MoreSectionVisibility == Visibility.Visible)
            {
                this.MoreSectionVisibility = Visibility.Collapsed;
                this.MoreLessText = MoreText;
            }
            else
            {
                this.MoreSectionVisibility = Visibility.Visible;
                this.MoreLessText = LessText;
                this.RefreshDiscussions().ConfigureAwait(false);
            }
        }

        private async Task RefreshDiscussions()
        {
            this.Discussions.Clear();
            var discussions = await this.mainViewModel.GetDiscussions(this.change);
            foreach(var discussion in discussions)
            {
                this.Discussions.Add(discussion);
            }
        }
    }
}
