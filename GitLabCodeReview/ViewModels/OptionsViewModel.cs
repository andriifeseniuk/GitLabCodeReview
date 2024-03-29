﻿using GitLabCodeReview.Models;

namespace GitLabCodeReview.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private OptionsModel options;

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

        public bool AutoCleanWorkingDirectory
        {
            get
            {
                return this.options?.AutoCleanWorkingDirectory ?? false;
            }
        }

        public void RefreshOptions(OptionsModel gitLabOptions)
        {
            this.options = gitLabOptions;
            this.SchedulePropertyChanged(nameof(this.ApiUrl));
            this.SchedulePropertyChanged(nameof(this.SelectedProjectId));
            this.SchedulePropertyChanged(nameof(this.RepositoryLocalPath));
            this.SchedulePropertyChanged(nameof(this.WorkingDirectory));
            this.SchedulePropertyChanged(nameof(this.AutoCleanWorkingDirectory));
        }
    }
}
