using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace GitLabCodeReview
{
    [CLSCompliant(false)]
    [ComVisible(true)]
    [Guid("9da1752d-b4ca-48ae-b6d4-d0e67f7bd4f3")]
    public class GitLabOptions : DialogPage
    {
        public string ApiUrl { get; set; }
        public string PrivateToken { get; set; }
        public int? SelectedProjectId { get; set; }
        public string RepositoryLocalPath { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
