using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class LineViewModel : BaseViewModel, IParentTreeNode
    {
        private bool isExpanded;

        public LineViewModel(int numberInChanges, int? numberInSourceFile, int? numberInTargetFile, string text)
        {
            this.NumberInChanges = numberInChanges;
            this.NumberInSourceFile = numberInSourceFile;
            this.NumberInTargetFile = numberInTargetFile;
            this.Text = text;
        }

        public LineViewModel(int numberInChanges, int? numberInSourceFile, int? numberInTargetFile, string text, LineDetailsViewModel details)
            : this(numberInChanges, numberInSourceFile, numberInTargetFile, text)
        {
            this.Details = details;
            this.Items.Add(details);
        }

        public int NumberInChanges { get; private set; }

        public int? NumberInSourceFile { get; private set; }

        public int? NumberInTargetFile { get; private set; }

        public string Text { get; private set; }

        public bool IsAdded => this.NumberInSourceFile != null && this.NumberInTargetFile == null;

        public bool IsRemoved => this.NumberInSourceFile == null && this.NumberInTargetFile != null;

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

        public LineDetailsViewModel Details { get; private set; }

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
