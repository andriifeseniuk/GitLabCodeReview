namespace GitLabCodeReview.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private GitLabOptions options;

        public string ApiUrl
        {
            get
            {
                return this.options?.ApiUrl;
            }
        }

        public long? SelectedProjectId
        {
            get
            {
                return this.options?.SelectedProjectId;
            }
        }

        public string RepositoryLocalPath
        {
            get
            {
                return this.options?.RepositoryLocalPath;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return this.options?.WorkingDirectory;
            }
        }

        public void RefreshOptions(GitLabOptions gitLabOptions)
        {
            this.options = gitLabOptions;
            this.SchedulePropertyChanged(nameof(this.ApiUrl));
            this.SchedulePropertyChanged(nameof(this.SelectedProjectId));
            this.SchedulePropertyChanged(nameof(this.WorkingDirectory));
            this.SchedulePropertyChanged(nameof(this.RepositoryLocalPath));
        }
    }
}
