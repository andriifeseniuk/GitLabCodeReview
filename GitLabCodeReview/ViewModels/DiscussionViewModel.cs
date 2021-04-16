using GitLabCodeReview.DTO;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace GitLabCodeReview.ViewModels
{
    public class DiscussionViewModel : BaseViewModel, IParentTreeNode
    {
        private bool isExpanded;

        public DiscussionViewModel(DiscussionDto gitLabDiscussion, GitLabService gitLabService)
        {
            this.Discussion = gitLabDiscussion;
            this.Details = new DiscussionDetailsViewModel(gitLabDiscussion, gitLabService);
            this.Items.Add(this.Details);
        }

        public string DisplayName
        {
            get
            {
                return this.Discussion.Notes.First().Body;
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
            }
        }

        public ObservableCollection<ITreeNode> Items { get; private set; } = new ObservableCollection<ITreeNode>();

        public DiscussionDto Discussion { get; }

        public DiscussionDetailsViewModel Details { get; private set; }
    }
}
