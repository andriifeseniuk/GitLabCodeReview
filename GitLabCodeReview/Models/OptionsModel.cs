using System;
using System.Collections.Generic;
using System.Linq;
namespace GitLabCodeReview.Models
{
    public class OptionsModel
    {
        public string ApiUrl { get; set; }
        public string PrivateToken { get; set; }
        public long? SelectedProjectId { get; set; }
        public string FavoriteProjects { get; set; }
        public string RepositoryLocalPath { get; set; }
        public string WorkingDirectory { get; set; }
        public bool AutoCleanWorkingDirectory { get; set; }
        public int? MaxItemsPerPage { get; set; }
        public int? MaxPages { get; set; }
    }
}
