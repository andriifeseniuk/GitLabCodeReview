using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class LineDetailsViewModel : BaseViewModel, ITreeNode
    {
        private readonly ObservableCollection<DiscussionViewModel> discussions = new ObservableCollection<DiscussionViewModel>();
        private bool isExpanded;
        private string newDiscussionText;
        private MergeRequestDetailsDto mergeRequest;
        private GitLabService service;
        private ChangeDto change;

        public LineDetailsViewModel (int? numberInSourceFile, int? numberInTargetFile, MergeRequestDetailsDto mergeRequestDto, ChangeDto changeDto, GitLabService gitLabService)
        {
            this.NumberInSourceFile = numberInSourceFile;
            this.NumberInTargetFile = numberInTargetFile;
            this.mergeRequest = mergeRequestDto;
            this.change = changeDto;
            this.service = gitLabService;
            this.NewDiscussionCommand = new DelegateCommand(this.ExecuteNewDiscussion);
            this.CancelCommand = new DelegateCommand(this.ExecuteCancel);
        }


        public int? NumberInSourceFile { get; private set; }

        public int? NumberInTargetFile { get; private set; }

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

        public string DisplayName
        {
            get
            {
                return string.Empty;
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
                this.SchedulePropertyChanged(nameof(this.IsAnyText));
            }
        }

        public bool IsAnyText => !string.IsNullOrEmpty(this.newDiscussionText);

        public DelegateCommand NewDiscussionCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        private async void ExecuteNewDiscussion(object obj)
        {
            var position = new PositionDto
            {
                PositionType = "text",
                BaseSha = this.mergeRequest.DiffRefs.BaseSha,
                StartSha = this.mergeRequest.DiffRefs.StartSha,
                HeadSha = this.mergeRequest.DiffRefs.HeadSha,
                NewPath = this.change.NewPath,
                OldPath = this.change.OldPath,
                NewLine = this.NumberInSourceFile,
                OldLine = this.NumberInTargetFile
            };

            var createDiscussionDto = new CreateDiscussionDto { Position = position };
            var dissDto = await this.service.AddDiscussion(createDiscussionDto, this.NewDiscussionText);
            var dissVm = new DiscussionViewModel(dissDto, this.service);
            this.Discussions.Add(dissVm);
            this.NewDiscussionText = string.Empty;
        }

        private void ExecuteCancel(object obj)
        {
            this.NewDiscussionText = string.Empty;
        }
    }
}
