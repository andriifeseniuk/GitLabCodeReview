using GitLabCodeReview.Models;
using System.IO;

namespace GitLabCodeReview.Helpers
{
    public static class DirectoryHelper
    {
        public const string MergeRequestDirectoryName = "MergeRequest";

        public static string GetWorkingOrTempDirectory(this OptionsModel options)
        {
            var workingDirectory = !string.IsNullOrWhiteSpace(options.WorkingDirectory)
                ? options.WorkingDirectory
                : Path.Combine(Path.GetTempPath(), "GitLabCodeReview");
            return workingDirectory;
        }
    }
}
