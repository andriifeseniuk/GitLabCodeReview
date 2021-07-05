using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GitLabCodeReview
{
    [CLSCompliant(false)]
    [ComVisible(true)]
    [Guid("9da1752d-b4ca-48ae-b6d4-d0e67f7bd4f3")]
    public class GitLabOptions : DialogPage
    {
        [DisplayName("API URL")]
        [Description("The URL of GitLab server API. This field is mandatory.")]
        public string ApiUrl { get; set; }

        [DisplayName("Private Token")]
        [Description("Your Private Access Token to autenticate to GitLab API. Keep this token secret. Authors are not responsible for any security issues. This field is mandatory.")]
        public string PrivateToken { get; set; }

        [DisplayName("Selected Project Id")]
        [Description("The ID of GitLab project you are working with. You can add it manually or choose it from the projects list. This field is optional.")]
        public long? SelectedProjectId { get; set; }

        [DisplayName("Favorite Projects")]
        [Description("The comma-separated list of GitLab project IDs you are working with. You can add it manually or choose it from the projects list. This field is optional.")]
        public string FavoriteProjects { get; set; }

        [DisplayName("Repository Local Path")]
        [Description("The local path to your git repository root. Used to diff files from remote against your local files if you current branch is the same as in merge request. This field is optional.")]
        public string RepositoryLocalPath { get; set; }

        [DisplayName("Working Directory")]
        [Description("The directory to keep temporary files used for diff. By default it is 'GitLabCodeReview' folder under your user directory. This field is optional.")]
        public string WorkingDirectory { get; set; }

        [DisplayName("Auto-clean Working Directory")]
        [Description("The flag indicating wheter to clean the working directory")]
        public bool AutoCleanWorkingDirectory { get; set; }

        [DisplayName("Max items per page")]
        [Description("The maximum number of items that can be loaded in one request to endpoint that supports pagination. By default 100. This field is optional.")]
        public int? MaxItemsPerPage { get; set; }

        [DisplayName("Max pages")]
        [Description("The maximum number of sequential requests to endpoint that supports pagination. By default 1000. This field is optional.")]
        public int? MaxPages { get; set; }
    }
}
