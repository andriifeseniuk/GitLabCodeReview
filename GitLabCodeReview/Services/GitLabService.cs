﻿using EnvDTE;
using GitLabCodeReview.Client;
using GitLabCodeReview.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GitLabCodeReview.Services
{
    public class GitLabService
    {
        private long? userId;
        private string userName;
        private ErrorService errorService;

        public GitLabService(ErrorService globalErrorService)
        {
            this.errorService = globalErrorService;
        }

        public GitLabOptions GitOptions { get; } = new GitLabOptions();

        public long? UserId
        {
            get
            {
                return this.userId;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
        }

        public long? SelectedProjectId => this.GitOptions.SelectedProjectId;

        public long? SelectedMergeRequestInternalId { get; set; }

        public async Task<string> GetFileContentAsync(string branch, string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branch))
                {
                    throw new ArgumentNullException(nameof(branch));
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var file = await client.GetFileAsync(this.SelectedProjectId.Value, branch, path);
                    var fileBlob = await client.GetFileBlobAsync(this.SelectedProjectId.Value, file.BlobId);
                    var fileContent = Encoding.UTF8.GetString(Convert.FromBase64String(fileBlob.Content));
                    return fileContent;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
                return null;
            }
        }

        public async Task<GitLabDiscussion[]> GetDiscussionsAsync(GitLabChange change)
        {
            try
            {
                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                if (this.SelectedMergeRequestInternalId == null)
                {
                    throw new InvalidOperationException("SelectedMergeRequestInternalId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    if (change.IsNewFile)
                    {
                        // TODO
                        return new GitLabDiscussion[0];
                    }

                    if (change.IsDeletedFile)
                    {
                        // TODO
                        return new GitLabDiscussion[0];
                    }

                    if (change.IsRenamedFile)
                    {
                        // TODO
                        return new GitLabDiscussion[0];
                    }

                    var discussions = await client.GetDiscussionsAsync(this.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value);
                    return discussions;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }

            return new GitLabDiscussion[0];
        }

        public void RefreshOptions()
        {
            try
            {
                var serviceProvoder = GitLabMainWindowCommand.Instance.ServiceProvider;
                var dte = (DTE)serviceProvoder.GetService(typeof(DTE));
                EnvDTE.Properties props = dte.Properties["GitLab Code Review", "General"];
                this.GitOptions.ApiUrl = (string)props.Item(nameof(GitLabOptions.ApiUrl)).Value;
                this.GitOptions.PrivateToken = (string)props.Item(nameof(GitLabOptions.PrivateToken)).Value;
                this.GitOptions.SelectedProjectId = (int?)props.Item(nameof(GitLabOptions.SelectedProjectId)).Value;
                this.GitOptions.RepositoryLocalPath = (string)props.Item(nameof(GitLabOptions.RepositoryLocalPath)).Value;
                this.GitOptions.WorkingDirectory = (string)props.Item(nameof(GitLabOptions.WorkingDirectory)).Value;
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        public async Task RefreshUserInfo()
        {
            try
            {
                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var user = await client.GetUserAsync();
                    this.userId = user.Id;
                    this.userName = user.UserName;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        public async Task<IEnumerable<GitLabProject>> GetProjectsAsync()
        {
            try
            {
                if (this.UserId == null)
                {
                    throw new InvalidOperationException("UserId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var projects = await client.GetProjectsAsync(this.UserId.Value);
                    return projects;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
                return new GitLabProject[0];
            }
        }

        public async Task<IEnumerable<GitLabMergeRequest>> GetMergeRequestsAsync()
        {
            try
            {
                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var requests = await client.GetMergeRequestsAsync(this.GitOptions.SelectedProjectId.Value);
                    return requests;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
                return new GitLabMergeRequest[0];
            }
        }

        public async Task<GitLabMergeRequestDetails> GetMergeRequestDetailsAsync()
        {
            try
            {
                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                if (this.SelectedMergeRequestInternalId == null)
                {
                    throw new InvalidOperationException("SelectedMergeRequestInternalId is null");
                }

                using (var client = new GitLabClient(this.GitOptions.ApiUrl, this.GitOptions.PrivateToken))
                {
                    var details = await client.GetMergeRequestDetailsAsync(this.GitOptions.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value);
                    return details;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
                return null;
            }
        }
    }
}