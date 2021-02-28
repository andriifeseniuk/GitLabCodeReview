using GitLabCodeReview.DTO;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Specialized;
using GitLabCodeReview.Extensions;

namespace GitLabCodeReview.ViewModels
{
    public class DiscussionViewModel : BaseViewModel, IParentTreeNode
    {
        private bool isExpanded;

        public DiscussionViewModel(DiscussionDto gitLabDiscussion)
        {
            this.Discussion = gitLabDiscussion;
            this.Notes.CollectionChanged += this.OnNotesCollectionChanged;
        }

        private void OnNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Items.Clear();
            this.Items.AddRange(this.Notes.Skip(1));
        }

        public DiscussionDto Discussion { get; }

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

        public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();
    }
}
