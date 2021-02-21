using System;
using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class LineViewModel : BaseViewModel, IParentTreeNode
    {
        private readonly ObservableCollection<DiscussionViewModel> discussions = new ObservableCollection<DiscussionViewModel>();
        private bool isExpanded;

        public LineViewModel (int number, string text, bool isSourceBranch)
        {
            this.Number = number;
            this.Text = text;
            this.IsSourceBranch = isSourceBranch;
        }

        public int Number { get; private set; }

        public string Text { get; private set; }

        public bool IsSourceBranch { get; private set; }

        public ObservableCollection<DiscussionViewModel> Discussions
        {
            get
            {
                return this.discussions;
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

        public string DisplayName
        {
            get
            {
                return this.Text;
            }
        }
    }
}
