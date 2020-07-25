using GitLabCodeReview.ViewModels;

namespace GitLabCodeReview.Models
{
    public class GitLabMergeRequestViewModel : BaseViewModel
    {
        private bool isSelected;
            
        public GitLabMergeRequestViewModel(long id, long iid, string title, string srcBranch, string tgtBranch)
        {
            this.Id = id;
            this.InternalId = iid;
            this.Title = title;
            this.SourceBranch = srcBranch;
            this.TargetBranch = tgtBranch;
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;
                this.SchedulePropertyChanged();
            }
        }

        public long Id { get; }
        public long InternalId { get; }
        public string Title { get; }
        public string SourceBranch { get; }
        public string TargetBranch { get; }
    }
}
