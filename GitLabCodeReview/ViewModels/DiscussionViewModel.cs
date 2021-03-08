using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Extensions;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace GitLabCodeReview.ViewModels
{
    public class DiscussionViewModel : BaseViewModel, IParentTreeNode
    {
        private bool isExpanded;
        private string newNoteText;
        private GitLabService service;

        public DiscussionViewModel(DiscussionDto gitLabDiscussion, GitLabService gitLabService)
        {
            this.Discussion = gitLabDiscussion;
            this.service = gitLabService;
            this.Notes.CollectionChanged += this.OnNotesCollectionChanged;
            this.NewNoteCommand = new DelegateCommand(this.ExecuteNewNote);
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

        public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();

        public string NewNoteText
        {
            get
            {
                return this.newNoteText;
            }
            set
            {
                this.newNoteText = value;
                this.SchedulePropertyChanged();
            }
        }

        public DelegateCommand NewNoteCommand { get; private set; }

        private void OnNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Items.Clear();
            this.Items.AddRange(this.Notes.Skip(1));
        }

        private void ExecuteNewNote(object obj)
        {
            this.service.AddNote(this.Discussion.Id, this.NewNoteText).ConfigureAwait(false);
        }
    }
}
