using GitLabCodeReview.DTO;
using GitLabCodeReview.Extensions;
using GitLabCodeReview.Helpers;
using GitLabCodeReview.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GitLabCodeReview.Client
{
    public class GitLabClient : IDisposable
    {
        private readonly HttpClient client;
        private readonly JsonSerializerSettings settings;
        private readonly OptionsModel gitOptions;

        public GitLabClient(OptionsModel gitLabOptions)
        {
            if (gitLabOptions == null)
            {
                throw new ArgumentNullException(nameof(gitLabOptions));
            }

            if (string.IsNullOrEmpty(gitLabOptions.ApiUrl))
            {
                throw new ArgumentNullException(nameof(gitLabOptions.ApiUrl));
            }

            if (string.IsNullOrEmpty(gitLabOptions.PrivateToken))
            {
                throw new ArgumentNullException(nameof(gitLabOptions.PrivateToken));
            }

            this.gitOptions = gitLabOptions;

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", gitOptions.PrivateToken);

            this.settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task<UserDto> GetUserAsync()
        {
            var uri = $"{this.gitOptions.ApiUrl}/user";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var user = JsonHelper.Deserialize<UserDto>(responseAsString);
            return user;
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(long userId)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects?membership=true&simple=true";
            var projects = await this.GetPaged<ProjectDto>(uri);
            return projects;
        }

        public async Task<IEnumerable<MergeRequestDto>> GetMergeRequestsAsync(long projectId)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/merge_requests?state=opened";
            var requests = await this.GetPaged<MergeRequestDto>(uri);
            return requests;
        }

        public async Task<MergeRequestDetailsDto> GetMergeRequestDetailsAsync(long projectId, long mergeRequestInternalId)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/changes";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var details = JsonHelper.Deserialize<MergeRequestDetailsDto>(responseAsString);
            return details;
        }

        public async Task<FileDto> GetFileAsync(long projectId, string branch, string path)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/repository/files/{HttpUtility.UrlEncode(path)}?ref={branch}";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var file = JsonHelper.Deserialize<FileDto>(responseAsString);
            return file;
        }

        public async Task<BlobDto> GetFileBlobAsync(long projectId, string blobId)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/repository/blobs/{blobId}";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var blob = JsonHelper.Deserialize<BlobDto>(responseAsString);
            return blob;
        }

        public async Task<DiscussionDto[]> GetDiscussionsAsync(long projectId, long mergeRequestInternalId)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/discussions";
            var discussions = await this.GetPaged<DiscussionDto>(uri);
            return discussions;
        }

        public async Task<NoteDto> AddNote(long projectId, long mergeRequestInternalId, string discussionId, string body)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/discussions/{discussionId}/notes?body={body}";
            var response = await client.PostAsync(uri, new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            var responseAsString = await response.Content.ReadAsStringAsync();
            var note = JsonHelper.Deserialize<NoteDto>(responseAsString);
            return note;
        }

        public async Task<DiscussionDto> AddDiscussion(long projectId, long mergeRequestInternalId, CreateDiscussionDto createDiscussionDto, string body)
        {
            var uri = $"{this.gitOptions.ApiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/discussions?body={body}";
            var content = JsonConvert.SerializeObject(createDiscussionDto, this.settings);
            var response = await client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
            var responseAsString = await response.Content.ReadAsStringAsync();
            var discussion = JsonHelper.Deserialize<DiscussionDto>(responseAsString);
            return discussion;
        }

        private async Task<T[]> GetPaged<T>(string uri)
        {
            var result = new List<T>();
            var maxPerPage = this.gitOptions.MaxItemsPerPage.HasValue && this.gitOptions.MaxItemsPerPage.Value <= 100
                ? this.gitOptions.MaxItemsPerPage.Value
                : 100;
            var maxPages = this.gitOptions.MaxPages.HasValue && this.gitOptions.MaxPages.Value <= 1000
                ? this.gitOptions.MaxPages.Value
                : 1000;
            for(var i = 1; i <= maxPages; i++)
            {
                var uriPerPage = uri.Parameter("per_page", maxPerPage.ToString()).Parameter("page", i.ToString());
                var response = await client.GetAsync(uriPerPage);
                if (!response.IsSuccessStatusCode)
                {
                    break;
                }

                var responseAsString = await response.Content.ReadAsStringAsync();
                var page = JsonHelper.Deserialize<T[]>(responseAsString);
                if (page.Length == 0)
                {
                    break;
                }

                result.AddRange(page);

                IEnumerable<string> values;
                int totalPages;
                if (response.Headers.TryGetValues("x-total-pages", out values)
                    && values.Any()
                    && int.TryParse(values.First(), out totalPages)
                    && i == totalPages)
                {
                    break;
                }
            }

            return result.ToArray();
        }
    }
}
