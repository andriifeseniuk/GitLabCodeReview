using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Windows.Input;
using GitLabCodeReview.Common.Commands;

namespace GitLabCodeReview.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            this.Errors.CollectionChanged += Errors_CollectionChanged;
            this.RefreshOptionsCommand = new DelegateCommand(obj => this.RefreshOptions());
        }

        public GitLabOptionsViewModel GitOptions { get; set; } = new GitLabOptionsViewModel();

        public string ErrorsHeader => $"Errors ({this.Errors.Count})";

        public ObservableCollection<string> Errors { get; } = new ObservableCollection<string>();

        public ICommand RefreshOptionsCommand { get; }

        public void RefreshAll()
        {
            this.RefreshOptions();
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
                this.Errors.Add(ex.Message);
            }
        }
    }
}
