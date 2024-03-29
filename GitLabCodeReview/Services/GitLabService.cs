﻿using GitLabCodeReview.Client;
using GitLabCodeReview.DTO;
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
        private readonly ErrorService errorService;
        private readonly IOptionsService gitLabOptionsService;

        bool isPending;

        public GitLabService(IOptionsService optionsService, ErrorService globalErrorService)
        {
            this.gitLabOptionsService = optionsService;
            this.errorService = globalErrorService;
        }

        public event Action<bool> IsPendingChanged;

        public bool IsPending
        {
            get
            {
                return this.isPending;
            }
            set
            {
                this.isPending = value;
                this.IsPendingChanged?.Invoke(value);
            }
        }

        public OptionsModel GitOptions { get; private set; } = new OptionsModel();

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
            this.IsPending = true;
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

                using (var client = new GitLabClient(this.GitOptions))
                {
                    var file = await client.GetFileAsync(this.SelectedProjectId.Value, branch, path);
                    var fileBlob = await client.GetFileBlobAsync(this.SelectedProjectId.Value, file.BlobId);
                    var fileContent = Encoding.UTF8.GetString(Convert.FromBase64String(fileBlob.Content));
                    return fileContent;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
                return null;
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task<DiscussionDto[]> GetDiscussionsAsync()
        {
            this.IsPending = true;
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

                using (var client = new GitLabClient(this.GitOptions))
                {
                    var discussions = await client.GetDiscussionsAsync(this.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value);
                    return discussions;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
            }
            finally
            {
                this.IsPending = false;
            }

            return new DiscussionDto[0];
        }

        public void RefreshOptions()
        {
            this.IsPending = true;
            try
            {
                this.GitOptions = this.gitLabOptionsService.LoadOptions();

            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public void SaveOptions()
        {
            this.IsPending = true;
            try
            {
                this.gitLabOptionsService.SaveOptions(this.GitOptions);
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task RefreshUserInfo()
        {
            this.IsPending = true;
            try
            {
                using (var client = new GitLabClient(this.GitOptions))
                {
                    var user = await client.GetUserAsync();
                    this.userId = user.Id;
                    this.userName = user.Name;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync()
        {
            this.IsPending = true;
            try
            {
                if (this.UserId == null)
                {
                    throw new InvalidOperationException("UserId is null");
                }

                using (var client = new GitLabClient(this.GitOptions))
                {
                    var projects = await client.GetProjectsAsync(this.UserId.Value);
                    return projects;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
                return new ProjectDto[0];
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task<IEnumerable<MergeRequestDto>> GetMergeRequestsAsync()
        {
            this.IsPending = true;
            try
            {
                if (this.SelectedProjectId == null)
                {
                    throw new InvalidOperationException("SelectedProjectId is null");
                }

                using (var client = new GitLabClient(this.GitOptions))
                {
                    var requests = await client.GetMergeRequestsAsync(this.GitOptions.SelectedProjectId.Value);
                    return requests;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
                return new MergeRequestDto[0];
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task<MergeRequestDetailsDto> GetMergeRequestDetailsAsync()
        {
            this.IsPending = true;
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

                using (var client = new GitLabClient(this.GitOptions))
                {
                    var details = await client.GetMergeRequestDetailsAsync(this.GitOptions.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value);
                    return details;
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
                return null;
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task<NoteDto> AddNote(string discussionId, string body)
        {
            this.IsPending = true;
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

                using (var client = new GitLabClient(this.GitOptions))
                {
                    return await client.AddNote(this.GitOptions.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value, discussionId, body);
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
                return null;
            }
            finally
            {
                this.IsPending = false;
            }
        }

        public async Task<DiscussionDto> AddDiscussion(CreateDiscussionDto createDiscussionDto, string body)
        {
            this.IsPending = true;
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

                using (var client = new GitLabClient(this.GitOptions))
                {
                    return await client.AddDiscussion(this.GitOptions.SelectedProjectId.Value, this.SelectedMergeRequestInternalId.Value, createDiscussionDto, body);
                }
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex);
                return null;
            }
            finally
            {
                this.IsPending = false;
            }
        }
    }
}
