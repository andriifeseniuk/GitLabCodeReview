using System.Collections.ObjectModel;
using System.Windows.Input;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.Models;
using System.Threading.Tasks;
using GitLabCodeReview.Services;

namespace GitLabCodeReview.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ErrorService errorService;
        private readonly GitLabService gitLabService;

        public MainViewModel()
        {
            this.errorService = new ErrorService();
            this.gitLabService = new GitLabService(this.errorService);
            this.errorService.Errors.CollectionChanged += Errors_CollectionChanged;
            this.RefreshOptionsCommand = new DelegateCommand(obj => this.RefreshOptions());
        }

        public GitLabOptionsViewModel GitOptions { get; set; } = new GitLabOptionsViewModel();

        public long? UserId => this.gitLabService.UserId;

        public string UserName => this.gitLabService.UserName;

        public ObservableCollection<GitLabProjectViewModel> Projects { get; } = new ObservableCollection<GitLabProjectViewModel>();

        public long? SelectedProjectId
        {
            get
            {
                return this.gitLabService.GitOptions.SelectedProjectId;
            }
            set
            {
                this.gitLabService.GitOptions.SelectedProjectId = value;
                this.SchedulePropertyChanged();
                this.RefreshMergeRequests().ConfigureAwait(false);
            }
        }

        public ObservableCollection<GitLabMergeRequestViewModel> MergeRequests { get; } = new ObservableCollection<GitLabMergeRequestViewModel>();

        public long? SelectedMergeRequestInternalId
        {
            get
            {
                return this.gitLabService.SelectedMergeRequestInternalId;
            }
            set
            {
                this.gitLabService.SelectedMergeRequestInternalId = value;
                this.SchedulePropertyChanged();
                this.RefreshChanges().ConfigureAwait(false);
            }
        }

        public ObservableCollection<GitLabChangeViewModel> Changes { get; } = new ObservableCollection<GitLabChangeViewModel>();

        public string ErrorsHeader => $"Errors ({this.Errors.Count})";

        public ObservableCollection<string> Errors => this.errorService.Errors;

        public ICommand RefreshOptionsCommand { get; }

        public async void RefreshAll()
        {
            this.RefreshOptions();
            await this.RefreshUserInfo();
            await this.RefreshProjects();
        }

        private void Errors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SchedulePropertyChanged(nameof(this.ErrorsHeader));
        }

        private void RefreshOptions()
        {
            this.gitLabService.RefreshOptions();
            this.GitOptions.RefreshOptions(this.gitLabService.GitOptions);
        }

        private async Task RefreshUserInfo()
        {
            await this.gitLabService.RefreshUserInfo();
            this.SchedulePropertyChanged(nameof(this.UserId));
            this.SchedulePropertyChanged(nameof(this.UserName));
        }

        private async Task RefreshProjects()
        {
            foreach (var project in this.Projects)
            {
                project.PropertyChanged -= this.OnProjectPropertyChanged;
            }

            this.Projects.Clear();

            var projects = await gitLabService.GetProjectsAsync();
            foreach (var project in projects)
            {
                this.Projects.Add(new GitLabProjectViewModel(project.Id, project.Name));
            }

            foreach (var project in this.Projects)
            {
                project.PropertyChanged += this.OnProjectPropertyChanged;
            }
        }

        private async Task RefreshMergeRequests()
        {
            foreach (var request in this.MergeRequests)
            {
                request.PropertyChanged -= this.OnMergeRequestPropertyChanged;
            }

            this.MergeRequests.Clear();

            var requests = await this.gitLabService.GetMergeRequestsAsync();
            foreach (var request in requests)
            {
                this.MergeRequests.Add(new GitLabMergeRequestViewModel(request.Id, request.InternalId, request.Title, request.SourceBranch, request.TargetBranch));
            }

            foreach (var request in this.MergeRequests)
            {
                request.PropertyChanged += this.OnMergeRequestPropertyChanged;
            }
        }

        private async Task RefreshChanges()
        {
            this.Changes.Clear();

            var details = await this.gitLabService.GetMergeRequestDetailsAsync();
            foreach (var change in details.Changes)
            {
                this.Changes.Add(new GitLabChangeViewModel(change, details, this.gitLabService, this.errorService));
            }
        }

        private void OnProjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var project = sender as GitLabProjectViewModel;
            if (project == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(GitLabProjectViewModel.IsSelected):
                {
                    if (project.IsSelected)
                    {
                        this.SelectedProjectId = project.Id;
                    }

                    break;
                }
            }
        }

        private void OnMergeRequestPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var MergeRequest = sender as GitLabMergeRequestViewModel;
            if (MergeRequest == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(GitLabMergeRequestViewModel.IsSelected):
                    {
                        if (MergeRequest.IsSelected)
                        {
                            this.SelectedMergeRequestInternalId = MergeRequest.InternalId;
                        }

                        break;
                    }
            }
        }
    }
}
