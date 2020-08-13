using System;
using System.Collections.ObjectModel;
using EnvDTE;
using System.Windows.Input;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.Models;
using System.Threading.Tasks;
using GitLabCodeReview.Client;
using GitLabCodeReview.Services;
using System.Linq;
using System.Text;
using System.IO;

namespace GitLabCodeReview.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private long? userId;
        private string userName;
        private long? selectedMergeRequestId;
        private ErrorService errorService; 

        public MainViewModel()
        {
            this.errorService = new ErrorService();
            this.errorService.Errors.CollectionChanged += Errors_CollectionChanged;
            this.RefreshOptionsCommand = new DelegateCommand(obj => this.RefreshOptions());
        }

        public GitLabOptionsViewModel GitOptions { get; set; } = new GitLabOptionsViewModel();

        public long? UserId
        {
            get
            {
                return this.userId;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
        }

        public ObservableCollection<GitLabProjectViewModel> Projects { get; } = new ObservableCollection<GitLabProjectViewModel>();

        public long? SelectedProjectId
        {
            get
            {
                return this.GitOptions.SelectedProjectId;
            }
            set
            {
                this.GitOptions.SelectedProjectId = value;
                this.SchedulePropertyChanged();
                this.RefreshMergeRequests().ConfigureAwait(false);
            }
        }

        public ObservableCollection<GitLabMergeRequestViewModel> MergeRequests { get; } = new ObservableCollection<GitLabMergeRequestViewModel>();

        public long? SelectedMergeRequestInternalId
        {
            get
            {
                return this.selectedMergeRequestId;
            }
            set
            {
                this.selectedMergeRequestId = value;
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

        public async void ExecuteDiff(GitLabChange change)
        {
            try
            {

                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                if (this.SelectedMergeRequestInternalId == null)
                {
                    throw new InvalidOperationException("SelectedMergeRequestInternalId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    if (change.IsNewFile)
                    {
                        // TODO
                        return;
                    }

                    if (change.IsDeletedFile)
                    {
                        // TODO
                        return;
                    }

                    if (change.IsRenamedFile)
                    {
                        // TODO
                        return;
                    }

                    var mergeRequest = this.MergeRequests.First(r => r.InternalId == this.SelectedMergeRequestInternalId);
                    var sourceFile = await client.GetFileAsync(this.SelectedProjectId.Value, mergeRequest.SourceBranch, change.NewPath);
                    var sourceFileBlob = await client.GetFileBlobAsync(this.SelectedProjectId.Value, sourceFile.BlobId);
                    var sourceFileContent = Encoding.UTF8.GetString(Convert.FromBase64String(sourceFileBlob.Content));

                    var targetFile = await client.GetFileAsync(this.SelectedProjectId.Value, mergeRequest.TargetBranch, change.NewPath);
                    var targetFileBlob = await client.GetFileBlobAsync(this.SelectedProjectId.Value, targetFile.BlobId);
                    var targetFileContent = Encoding.UTF8.GetString(Convert.FromBase64String(targetFileBlob.Content));

                    var dir = Path.Combine(this.GitOptions.WorkingDirectory, $"MergeRequest{mergeRequest.Id}");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var changeDir = Path.GetDirectoryName(change.NewPath);
                    var fileName = Path.GetFileNameWithoutExtension(change.NewPath);
                    var extension = Path.GetExtension(change.NewPath);

                    var sourceFileName = $"{fileName}_source_{extension}";
                    var targetFileName = $"{fileName}_target_{extension}";

                    var sourceFileLocalPath = Path.Combine(dir, sourceFileName);
                    var sourceFileLocalDir = Path.GetDirectoryName(sourceFileLocalPath);
                    if (!Directory.Exists(sourceFileLocalDir))
                    {
                        Directory.CreateDirectory(sourceFileLocalDir);
                    }

                    File.WriteAllText(sourceFileLocalPath, sourceFileContent);

                    var targetFileLocaPath = Path.Combine(dir, targetFileName);
                    var targetFileLocalDir = Path.GetDirectoryName(targetFileLocaPath);
                    if (!Directory.Exists(targetFileLocalDir))
                    {
                        Directory.CreateDirectory(targetFileLocalDir);
                    }

                    File.WriteAllText(targetFileLocaPath, targetFileContent);

                    var serviceProvoder = GitLabMainWindowCommand.Instance.ServiceProvider;
                    var dte = (DTE)serviceProvoder.GetService(typeof(DTE));
                    var arg = $"\"{targetFileLocaPath}\" \"{sourceFileLocalPath}\"";
                    dte.ExecuteCommand("Tools.DiffFiles", arg);
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        public async Task<GitLabDiscussion[]> GetDiscussions(GitLabChange change)
        {
            try
            {
                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                if (this.SelectedMergeRequestInternalId == null)
                {
                    throw new InvalidOperationException("SelectedMergeRequestInternalId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    if (change.IsNewFile)
                    {
                        // TODO
                        return new GitLabDiscussion[0];
                    }

                    if (change.IsDeletedFile)
                    {
                        // TODO
                        return new GitLabDiscussion[0];
                    }

                    if (change.IsRenamedFile)
                    {
                        // TODO
                        return new GitLabDiscussion[0];
                    }

                    var discussions = await client.GetDiscussionsAsync(this.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value);
                    return discussions;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }

            return new GitLabDiscussion[0];
        }

        private void Errors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SchedulePropertyChanged(nameof(this.ErrorsHeader));
        }

        private void RefreshOptions()
        {
            try
            {
                var serviceProvoder = GitLabMainWindowCommand.Instance.ServiceProvider;
                var dte = (DTE)serviceProvoder.GetService(typeof(DTE));
                EnvDTE.Properties props = dte.Properties["GitLab Code Review", "General"];
                this.GitOptions.ApiUrl = (string)props.Item(nameof(GitLabOptions.ApiUrl)).Value;
                this.GitOptions.PrivateToken = (string)props.Item(nameof(GitLabOptions.PrivateToken)).Value;
                this.GitOptions.SelectedProjectId = (int?)props.Item(nameof(GitLabOptions.SelectedProjectId)).Value;
                this.GitOptions.RepositoryLocalPath = (string)props.Item(nameof(GitLabOptions.RepositoryLocalPath)).Value;
                this.GitOptions.WorkingDirectory = (string)props.Item(nameof(GitLabOptions.WorkingDirectory)).Value;
            }
            catch(Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        private async Task RefreshUserInfo()
        {
            try
            {
                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var user = await client.GetUserAsync();
                    this.userId = user.Id;
                    this.userName = user.UserName;
                }

                this.SchedulePropertyChanged(nameof(this.UserId));
                this.SchedulePropertyChanged(nameof(this.UserName));
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        private async Task RefreshProjects()
        {
            try
            {
                foreach (var project in this.Projects)
                {
                    project.PropertyChanged -= this.OnProjectPropertyChanged;
                }

                this.Projects.Clear();

                if (this.UserId == null)
                {
                    throw new InvalidOperationException("UserId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var projects = await client.GetProjectsAsync(this.UserId.Value);
                    foreach (var project in projects)
                    {
                        this.Projects.Add(new GitLabProjectViewModel(project.Id, project.Name));
                    }
                }

                foreach (var project in this.Projects)
                {
                    project.PropertyChanged += this.OnProjectPropertyChanged;
                }

            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        private async Task RefreshMergeRequests()
        {
            try
            {
                foreach (var request in this.MergeRequests)
                {
                    request.PropertyChanged -= this.OnMergeRequestPropertyChanged;
                }

                this.MergeRequests.Clear();

                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var requests = await client.GetMergeRequestsAsync(this.GitOptions.SelectedProjectId.Value);
                    foreach (var request in requests)
                    {
                        this.MergeRequests.Add(new GitLabMergeRequestViewModel(request.Id, request.InternalId, request.Title, request.SourceBranch, request.TargetBranch));
                    }
                }

                foreach (var request in this.MergeRequests)
                {
                    request.PropertyChanged += this.OnMergeRequestPropertyChanged;
                }

            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        private async Task RefreshChanges()
        {
            try
            {
                this.Changes.Clear();

                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                if (this.SelectedMergeRequestInternalId == null)
                {
                    throw new InvalidOperationException("SelectedMergeRequestInternalId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var details = await client.GetMergeRequestDetailsAsync(this.GitOptions.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value);
                    foreach (var change in details.Changes)
                    {
                        this.Changes.Add(new GitLabChangeViewModel(change, this));
                    }
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
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
