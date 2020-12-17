using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class FolderViewModel : BaseViewModel, IParentTreeNode
    {
        private bool isExpanded;

        public FolderViewModel(string displayName)
        {
            this.DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

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
    }
}
