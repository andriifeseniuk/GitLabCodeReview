using System;
using System.Collections.ObjectModel;
using EnvDTE;
using System.Windows.Input;
using GitLabCodeReview.Common.Commands;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GitLabCodeReview.Models;
using System.Threading.Tasks;
using GitLabCodeReview.Client;

namespace GitLabCodeReview.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private long? userId;
        private string userName;

        public MainViewModel()
        {
            this.Errors.CollectionChanged += Errors_CollectionChanged;
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

        public string ErrorsHeader => $"Errors ({this.Errors.Count})";

        public ObservableCollection<string> Errors { get; } = new ObservableCollection<string>();

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
                this.AddError(ex.ToString());
            }
        }

        private async Task RefreshUserInfo()
        {
            try
            {
                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var user = await client.GetUserAsync();
                    this.userId = user.UserId;
                    this.userName = user.UserName;
                }

                this.SchedulePropertyChanged(nameof(this.UserId));
                this.SchedulePropertyChanged(nameof(this.UserName));
            }
            catch (Exception ex)
            {
                this.AddError(ex.ToString());
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
                    var projects = await client.GetProjects(this.UserId.Value);
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
                this.AddError(ex.ToString());
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
                        this.GitOptions.SelectedProjectId = project.Id;
                    }

                    break;
                }
            }
        }

        private void AddError(string message)
        {
            this.Errors.Add(message);
        }
    }
}
