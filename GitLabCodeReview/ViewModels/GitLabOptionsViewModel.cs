using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabOptionsViewModel : BaseViewModel
    {
        private string apiUrl;
        private string privateToken;
        private long? selectedProjectId;
        private string repositoryLocalPath;
        private string workingDirectory;

        public string ApiUrl
        {
            get
            {
                return apiUrl;
            }
            set
            {
                this.apiUrl = value;
                this.SchedulePropertyChanged();
            }
        }

        public string PrivateToken
        {
            get
            {
                return privateToken;
            }
            set
            {
                this.privateToken = value;
                this.SchedulePropertyChanged();
            }
        }

        public long? SelectedProjectId
        {
            get
            {
                return this.selectedProjectId;
            }
            set
            {
                this.selectedProjectId = value;
                this.SchedulePropertyChanged();
            }
        }

        public string RepositoryLocalPath
        {
            get
            {
                return this.repositoryLocalPath;
            }
            set
            {
                this.repositoryLocalPath = value;
                this.SchedulePropertyChanged();
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return this.workingDirectory;
            }
            set
            {
                this.workingDirectory = value;
                this.SchedulePropertyChanged();
            }
        }
    }
}
