using GitLabCodeReview.DTO;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace GitLabCodeReview.ViewModels
{
    public class ChangeViewModel : BaseViewModel, IParentTreeNode
    {
        private readonly ChangeDto change;
        private readonly GitLabService service;
        private bool isExpanded;
        private readonly ChangeDetailsViewModel details;

        public ChangeViewModel(
            ChangeDto gitLabChange,
            MergeRequestDetailsDto gitLabMergeRequest,
            GitLabService service,
            ErrorService globalErrorService)
        {
            this.change = gitLabChange;
            this.service = service;
            this.details = new ChangeDetailsViewModel(gitLabChange, gitLabMergeRequest, service, globalErrorService);
            this.Items.Add(this.details);
        }

        public ICommand DiffCommand => this.details.DiffCommand;

        public string DisplayName
        {
            get
            {
                if (this.change.IsDeletedFile)
                {
                    return Path.GetFileName(this.change.OldPath);
                }

                if (this.change.IsRenamedFile)
                {
                    return $"{Path.GetFileName(this.change.NewPath)}[{Path.GetFileName(this.change.OldPath)}]";
                }

                return Path.GetFileName(this.change.NewPath);
            }
        }

        public string FullPath
        {
            get
            {
                if (this.change.IsDeletedFile)
                {
                    return this.change.OldPath;
                }

                return this.change.NewPath;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                this.isExpanded = value;
                this.SchedulePropertyChanged();

                if (this.isExpanded)
                {
                    this.details.ExecuteLoadLines();
                }
            }
        }

        public ObservableCollection<ITreeNode> Items { get; private set; } = new ObservableCollection<ITreeNode>();
    }
}
