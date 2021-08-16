using EnvDTE;
using GitLabCodeReview.Services;

namespace GitLabCodeReviewVsix2019
{
    public class DiffService : IDiffService
    {
        public void Diff(string targetFileLocaPath, string sourceFileLocalPath)
        {
            var serviceProvider = GitLabMainWindow2019Command.Instance.ServiceProvider;
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            var arg = $"\"{targetFileLocaPath}\" \"{sourceFileLocalPath}\"";
            dte.ExecuteCommand("Tools.DiffFiles", arg);
        }
    }
}
