using GitLabCodeReview.ViewModels;

namespace GitLabCodeReview.DTO
{
    public class MergeRequestViewModel : BaseViewModel
    {
        private bool isSelected;
            
        public MergeRequestViewModel(long id, long iid, string title, string createdAt, UserDto author, string srcBranch, string tgtBranch)
        {
            this.Id = id;
            this.InternalId = iid;
            this.Title = title;
            this.CreatedAt = createdAt;
            this.Author = author;
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
        public string CreatedAt { get; }
        public UserDto Author { get; }
        public string SourceBranch { get; }
        public string TargetBranch { get; }
    }
}
