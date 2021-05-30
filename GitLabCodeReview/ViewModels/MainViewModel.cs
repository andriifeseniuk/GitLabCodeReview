using System.Collections.ObjectModel;
using System.Windows.Input;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using System.Threading.Tasks;
using GitLabCodeReview.Services;
using System.Linq;
using System;
using System.IO;
using GitLabCodeReview.Helpers;

namespace GitLabCodeReview.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ErrorService errorService;
        private readonly GitLabService gitLabService;

        public MainViewModel()
        {
            this.errorService = new ErrorService();
            this.errorService.Errors.CollectionChanged += Errors_CollectionChanged;
            this.RefreshAllCommand = new DelegateCommand(obj => this.RefreshAll());
            this.gitLabService = new GitLabService(this.errorService);

            this.gitLabService.IsPendingChanged += OnIsPendingChanged;
        }

        public bool IsBusy => this.gitLabService.IsPending;

        public OptionsViewModel GitOptions { get; set; } = new OptionsViewModel();

        public long? UserId => this.gitLabService.UserId;

        public string UserName => this.gitLabService.UserName;

        public ObservableCollection<ProjectViewModel> Projects { get; } = new ObservableCollection<ProjectViewModel>();

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

        public ObservableCollection<MergeRequestViewModel> MergeRequests { get; } = new ObservableCollection<MergeRequestViewModel>();

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

        public FolderViewModel ChangesRoot { get; } = new FolderViewModel("Changes");

        public string ErrorsHeader => $"Errors ({this.Errors.Count})";

        public ObservableCollection<string> Errors => this.errorService.Errors;

        public ICommand RefreshAllCommand { get; }

        public async void RefreshAll()
        {
            this.RefreshOptions();
            await this.RefreshUserInfo();
            await this.RefreshProjects();
            this.RemoveOldDirectories();
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
                this.Projects.Add(new ProjectViewModel(project.Id, project.Name));
            }

            foreach (var project in this.Projects)
            {
                project.PropertyChanged += this.OnProjectPropertyChanged;
            }

            this.MergeRequests.Clear();
            this.ChangesRoot.Items.Clear();
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
                this.MergeRequests.Add(new MergeRequestViewModel(request.Id, request.InternalId, request.Title, request.CreatedAt, request.Author, request.SourceBranch, request.TargetBranch));
            }

            foreach (var request in this.MergeRequests)
            {
                request.PropertyChanged += this.OnMergeRequestPropertyChanged;
            }

            this.ChangesRoot.Items.Clear();
        }

        private void AddNode(FolderViewModel root, ITreeNode leaf, string[] parentsFolders)
        {
            var currentParent = root;
            foreach(var folderName in parentsFolders)
            {
                var nextParent = currentParent.Items.OfType<FolderViewModel>().FirstOrDefault(item => item.DisplayName == folderName);
                if (nextParent == null)
                {
                    nextParent = new FolderViewModel(folderName);
                    currentParent.Items.Add(nextParent);
                }

                currentParent = nextParent;
            }

            currentParent.Items.Add(leaf);
        }

        private async Task RefreshChanges()
        {
            this.ChangesRoot.Items.Clear();

            var details = await this.gitLabService.GetMergeRequestDetailsAsync();
            foreach (var change in details.Changes)
            {
                var leaf = new ChangeViewModel(change, details, this.gitLabService, this.errorService);
                var pathSplitList = leaf.FullPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                pathSplitList.RemoveAt(pathSplitList.Count - 1);
                this.AddNode(this.ChangesRoot, leaf, pathSplitList.ToArray());
            }
        }

        private void OnProjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var project = sender as ProjectViewModel;
            if (project == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(ProjectViewModel.IsSelected):
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
            var MergeRequest = sender as MergeRequestViewModel;
            if (MergeRequest == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(MergeRequestViewModel.IsSelected):
                    {
                        if (MergeRequest.IsSelected)
                        {
                            this.SelectedMergeRequestInternalId = MergeRequest.InternalId;
                        }

                        break;
                    }
            }
        }

        private void OnIsPendingChanged(bool isPending)
        {
            this.SchedulePropertyChanged(nameof(this.IsBusy));
        }

        private void RemoveOldDirectories()
        {
            if (!this.GitOptions.AutoCleanWorkingDirectory)
            {
                return;
            }

            try
            {
                var workingDirectory = this.gitLabService.GitOptions.GetWorkingOrTempDirectory();
                if (!Directory.Exists(workingDirectory))
                {
                    return;
                }

                var directories = Directory.EnumerateDirectories(workingDirectory, DirectoryHelper.MergeRequestDirectoryName + "*");
                foreach(var dir in directories)
                {
                    Directory.Delete(dir, true);
                }
            }
            catch(Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }
    }
}
