﻿using GitLabCodeReview.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitLabCodeReview.Models;
using EnvDTE;

namespace GitLabCodeReviewVsix2015
{
    public class OptionsService : IOptionsService
    {
        public OptionsModel LoadOptions()
        {
            var options = new OptionsModel();
            var serviceProvider = GitLabMainWindowCommand.Instance.ServiceProvider;
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            EnvDTE.Properties props = dte.Properties["GitLab Code Review 2015", "General"];
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

            var serviceProvider = GitLabMainWindowCommand.Instance.ServiceProvider;
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            EnvDTE.Properties props = dte.Properties["GitLab Code Review 2015", "General"];
            props.Item(nameof(GitLabOptions.SelectedProjectId)).Value = options.SelectedProjectId;
            props.Item(nameof(GitLabOptions.FavoriteProjects)).Value = options.FavoriteProjects;
        }
    }
}
