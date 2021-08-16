using GitLabCodeReview.Services;
using System;
using GitLabCodeReview.Models;
using EnvDTE;

namespace GitLabCodeReviewVsix2019
{
    public class OptionsService : IOptionsService
    {
        public OptionsModel LoadOptions()
        {
            var options = new OptionsModel();
            var serviceProvider = GitLabMainWindow2019Command.Instance.ServiceProvider;
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            EnvDTE.Properties props = dte.Properties["GitLab Code Review", "General"];
            options.ApiUrl = (string)props.Item(nameof(GitLabOptions.ApiUrl)).Value;
            options.PrivateToken = (string)props.Item(nameof(GitLabOptions.PrivateToken)).Value;
            options.SelectedProjectId = (long?)props.Item(nameof(GitLabOptions.SelectedProjectId)).Value;
            options.FavoriteProjects = (string)props.Item(nameof(GitLabOptions.FavoriteProjects)).Value;
            options.RepositoryLocalPath = (string)props.Item(nameof(GitLabOptions.RepositoryLocalPath)).Value;
            options.WorkingDirectory = (string)props.Item(nameof(GitLabOptions.WorkingDirectory)).Value;
            options.AutoCleanWorkingDirectory = (bool)props.Item(nameof(GitLabOptions.AutoCleanWorkingDirectory)).Value;
            options.MaxItemsPerPage = (int?)props.Item(nameof(GitLabOptions.MaxItemsPerPage)).Value;
            options.MaxPages = (int?)props.Item(nameof(GitLabOptions.MaxPages)).Value;
            return options;
        }

        public void SaveOptions(OptionsModel options)
        {

            var serviceProvider = GitLabMainWindow2019Command.Instance.ServiceProvider;
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            EnvDTE.Properties props = dte.Properties["GitLab Code Review", "General"];
            props.Item(nameof(GitLabOptions.SelectedProjectId)).Value = options.SelectedProjectId;
            props.Item(nameof(GitLabOptions.FavoriteProjects)).Value = options.FavoriteProjects;
        }
    }
}
