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
using GitLabCodeReview.Enums;
using System.Collections.Generic;
using GitLabCodeReview.Extensions;

namespace GitLabCodeReview.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ErrorService errorService;
        private readonly GitLabService gitLabService;
        ChangesDisplayOptions selectedChangeOption = ChangesDisplayOptions.Folders;
        ChangesDisplayOptions[] changesOptions = Enum.GetValues(typeof(ChangesDisplayOptions)).Cast<ChangesDisplayOptions>().ToArray();

        public MainViewModel()
        {
            this.errorService = new ErrorService();
            this.errorService.Errors.CollectionChanged += Errors_CollectionChanged;
            this.RefreshAllCommand = new DelegateCommand(obj => this.RefreshAll());
            this.SaveCommand = new DelegateCommand(obj => this.SaveOptions());
            this.gitLabService = new GitLabService(this.errorService);

            this.gitLabService.IsPendingChanged += OnIsPendingChanged;
        }

        public bool IsBusy => this.gitLabService.IsPending;

        public OptionsViewModel GitOptions { get; set; } = new OptionsViewModel();

        public long? UserId => this.gitLabService.UserId;

        public string UserName => this.gitLabService.UserName;

        public ObservableCollection<ProjectViewModel> Projects { get; } = new ObservableCollection<ProjectViewModel>();

        public ObservableCollection<ProjectViewModel> FavoriteProjects { get; } = new ObservableCollection<ProjectViewModel>();

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

        public ICommand SaveCommand { get; }

        public ChangesDisplayOptions[] ChangesOptions => this.changesOptions;

        public ChangesDisplayOptions SelectedChangeOption
        {
            get
            {
                return this.selectedChangeOption;
            }
            set
            {
                this.selectedChangeOption = value;
                this.SchedulePropertyChanged();
                this.RefreshChanges().ConfigureAwait(false);
            }
        }

        public async void RefreshAll()
        {
            this.RefreshOptions();
            await this.RefreshUserInfo();
            await this.RefreshProjects();
            this.RefreshFavoriteProjects();
            this.RemoveOldDirectories();
        }

        private void SaveOptions()
        {
            this.gitLabService.SaveOptions();
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
            foreach (var project in projects.OrderBy(p => p.Name))
            {
                var projectVm = new ProjectViewModel(project.Id, project.Name);
                this.Projects.Add(projectVm);
                if (this.GitOptions.SelectedProjectId.HasValue
                    && this.GitOptions.SelectedProjectId.Value == project.Id)
                {
                    projectVm.IsSelected = true;
                }

                if (this.gitLabService.GitOptions.IsFavoriteProject(project.Id))
                {
                    projectVm.IsFavorite = true;
                }
            }

            foreach (var project in this.Projects)
            {
                project.PropertyChanged += this.OnProjectPropertyChanged;
            }

            this.MergeRequests.Clear();
            this.ChangesRoot.Items.Clear();
        }

        private void RefreshFavoriteProjects()
        {
            this.FavoriteProjects.Clear();
            var newFavoriteProjects = this.Projects.Where(p => p.IsFavorite);
            this.FavoriteProjects.AddRange(newFavoriteProjects);
            var ids = newFavoriteProjects.Select(p => p.Id).ToArray();
            this.gitLabService.GitOptions.SetFavoriteProjects(ids);
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

        private async Task RefreshChanges()
        {
            this.ChangesRoot.Items.Clear();
            var projectName = this.Projects.First(p => p.Id == this.SelectedProjectId).Name;

            var details = await this.gitLabService.GetMergeRequestDetailsAsync();
            foreach (var change in details.Changes)
            {
                var leaf = new ChangeViewModel(change, details, projectName, this.gitLabService, this.errorService);
                var pathSplitList = this.GetPathSplitList(leaf.FullPath);
                this.AddNode(this.ChangesRoot, leaf, pathSplitList.ToArray());
            }
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

        private string[] GetPathSplitList(string fullPath)
        {
            switch(this.SelectedChangeOption)
            {
                case ChangesDisplayOptions.Files:
                    return new string[0];

                case ChangesDisplayOptions.Folders:
                    {
                        var pathSplitList = fullPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        pathSplitList.RemoveAt(pathSplitList.Count - 1);
                        if (pathSplitList.Count == 0)
                        {
                            return new string[0];
                        }

                        var folderName = pathSplitList.Last();
                        var folderPath = string.Join("\\", pathSplitList);
                        var folderDisplayName = $"{folderName} [{folderPath}]";
                        return new[] { folderDisplayName };
                    }

                case ChangesDisplayOptions.Tree:
                    {
                        var pathSplitList = fullPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        pathSplitList.RemoveAt(pathSplitList.Count - 1);
                        return pathSplitList.ToArray();
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(this.SelectedChangeOption));
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
                case nameof(ProjectViewModel.IsFavorite):
                    {
                        this.RefreshFavoriteProjects();
                        this.SaveOptions();
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
