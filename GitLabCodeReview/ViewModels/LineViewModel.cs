using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Extensions;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GitLabCodeReview.ViewModels
{
    public class LineViewModel : BaseViewModel, IParentTreeNode
    {
        private readonly ObservableCollection<DiscussionViewModel> discussions = new ObservableCollection<DiscussionViewModel>();
        private bool isExpanded;
        private string newDiscussionText;
        private MergeRequestDetailsDto mergeRequest;
        private GitLabService service;
        private ChangeDto change;
        private ITreeNode dummyItem = new DummyTreeNode { DisplayName = "No discussions to show" };

        public LineViewModel (int number, string text, bool isSourceBranch, MergeRequestDetailsDto mergeRequestDto, ChangeDto changeDto, GitLabService gitLabService)
        {
            this.Number = number;
            this.Text = text;
            this.IsSourceBranch = isSourceBranch;
            this.mergeRequest = mergeRequestDto;
            this.change = changeDto;
            this.service = gitLabService;
            this.Items.Add(this.dummyItem);
            this.discussions.CollectionChanged += this.OnDiscussionsCollectionChanged;
            this.NewDiscussionCommand = new DelegateCommand(this.ExecuteNewDiscussion);
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

        public string NewDiscussionText
        {
            get
            {
                return this.newDiscussionText;
            }
            set
            {
                this.newDiscussionText = value;
                this.SchedulePropertyChanged();
            }
        }

        public DelegateCommand NewDiscussionCommand { get; private set; }

        private void OnDiscussionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Items.Clear();
            this.Items.AddRange(this.discussions);

            if (this.Items.Count == 0)
            {
                this.Items.Add(this.dummyItem);
            }
        }

        private void ExecuteNewDiscussion(object obj)
        {
            var position = new PositionDto
            {
                PositionType = "text",
                BaseSha = this.mergeRequest.DiffRefs.BaseSha,
                StartSha = this.mergeRequest.DiffRefs.StartSha,
                HeadSha = this.mergeRequest.DiffRefs.HeadSha,
                NewPath = this.change.NewPath,
                OldPath = this.change.OldPath,
                NewLine = this.IsSourceBranch ? (int?)this.Number : null,
                OldLine = this.IsSourceBranch ? null : (int?)this.Number
            };

            var createDiscussionDto = new CreateDiscussionDto { Position = position };
            this.service.AddDiscussion(createDiscussionDto, this.NewDiscussionText).ConfigureAwait(false);
        }
    }
}
