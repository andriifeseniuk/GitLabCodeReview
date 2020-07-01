using System;
using System.Collections.ObjectModel;
using EnvDTE;
using System.Windows.Input;
using GitLabCodeReview.Common.Commands;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public string ErrorsHeader => $"Errors ({this.Errors.Count})";

        public ObservableCollection<string> Errors { get; } = new ObservableCollection<string>();

        public ICommand RefreshOptionsCommand { get; }

        public void RefreshAll()
        {
            this.RefreshOptions();
            this.RefreshUserInfo();
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

        private async void RefreshUserInfo()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", this.GitOptions.PrivateToken);
                var uri = $"{this.GitOptions.ApiUrl}/user";
                var response = await client.GetAsync(uri);
                var responseAsString = await response.Content.ReadAsStringAsync();
                var jToken = (JToken)JsonConvert.DeserializeObject(responseAsString);
                this.userId = jToken["id"].Value<long>();
                this.userName = jToken["username"].Value<string>();

                this.SchedulePropertyChanged(nameof(this.UserId));
                this.SchedulePropertyChanged(nameof(this.UserName));
            }
            catch (Exception ex)
            {
                this.AddError(ex.ToString());
            }
        }

        private void AddError(string message)
        {
            this.Errors.Add(message);
        }
    }
}
