using EnvDTE;
using GitLabCodeReview.Services;

namespace GitLabCodeReviewVsix2015
{
    public class DiffService : IDiffService
    {
        public void Diff(string targetFileLocaPath, string sourceFileLocalPath)
        {
            var serviceProvoder = GitLabMainWindowCommand.Instance.ServiceProvider;
            var dte = (DTE)serviceProvoder.GetService(typeof(DTE));
            var arg = $"\"{targetFileLocaPath}\" \"{sourceFileLocalPath}\"";
            dte.ExecuteCommand("Tools.DiffFiles", arg);
        }
    }
}
