using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class LineViewModel : BaseViewModel
    {
        private readonly ObservableCollection<DiscussionViewModel> discussions = new ObservableCollection<DiscussionViewModel>();

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
    }
}
