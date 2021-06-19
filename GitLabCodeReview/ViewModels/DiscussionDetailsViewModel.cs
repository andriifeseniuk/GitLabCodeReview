using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Extensions;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace GitLabCodeReview.ViewModels
{
    public class DiscussionDetailsViewModel : BaseViewModel, ITreeNode
    {
        private bool isExpanded;
        private string newNoteText;
        private GitLabService service;

        public DiscussionDetailsViewModel(DiscussionDto gitLabDiscussion, GitLabService gitLabService)
        {
            this.Discussion = gitLabDiscussion;
            this.service = gitLabService;
            this.Notes.CollectionChanged += this.OnNotesCollectionChanged;
            this.NewNoteCommand = new DelegateCommand(this.ExecuteNewNote);
            this.CancelCommand = new DelegateCommand(this.ExecuteCancel);
        }

        public string DisplayName
        {
            get
            {
                return string.Empty;
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
                this.SchedulePropertyChanged(nameof(this.IsAnyText));
            }
        }

        public bool IsAnyText => !string.IsNullOrEmpty(this.newNoteText);

        public DelegateCommand NewNoteCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        private void OnNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Items.Clear();
            this.Items.AddRange(this.Notes.Skip(1));
        }

        private async void ExecuteNewNote(object obj)
        {
            var noteDto = await this.service.AddNote(this.Discussion.Id, this.NewNoteText);
            var noteVm = new NoteViewModel(noteDto);
            this.Notes.Add(noteVm);
            this.NewNoteText = string.Empty;
        }

        private void ExecuteCancel(object obj)
        {
            this.NewNoteText = string.Empty;
        }
    }
}
