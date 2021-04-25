using GitLabCodeReview.DTO;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class LineViewModel : BaseViewModel, IParentTreeNode
    {
        private bool isExpanded;

        public LineViewModel (int number, string text, bool isSourceBranch, MergeRequestDetailsDto mergeRequestDto, ChangeDto changeDto, GitLabService gitLabService)
        {
            this.Number = number;
            this.Text = text;
            this.IsSourceBranch = isSourceBranch;
        }

        public int Number { get; private set; }

        public string Text { get; private set; }

        public bool IsSourceBranch { get; private set; }

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

        public LineDetailsViewModel Details { get; set; }

        public ObservableCollection<ITreeNode> Items { get; private set; } = new ObservableCollection<ITreeNode>();

        public string DisplayName
        {
            get
            {
                return this.Text;
            }
        }
    }
}
