using GitLabCodeReview.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GitLabCodeReview.Client
{
    public class GitLabClient : IDisposable
    {
        private readonly string apiUrl;
        private readonly HttpClient client;
        private readonly JsonSerializerSettings settings;

        public GitLabClient(string apiUrl, string privateToken)
        {
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new ArgumentNullException(nameof(apiUrl));
            }

            if (string.IsNullOrEmpty(privateToken))
            {
                throw new ArgumentNullException(nameof(privateToken));
            }

            this.apiUrl = apiUrl;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", privateToken);

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
            var uri = $"{this.apiUrl}/user";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var user = (UserDto)JsonConvert.DeserializeObject(responseAsString, typeof(UserDto));
            return user;
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(long userId)
        {
            var uri = $"{this.apiUrl}/users/{userId}/projects";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var projects = (ProjectDto[])JsonConvert.DeserializeObject(responseAsString, typeof(ProjectDto[]));
            return projects;
        }

        public async Task<IEnumerable<MergeRequestDto>> GetMergeRequestsAsync(long projectId)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/merge_requests?state=opened";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var requests = (MergeRequestDto[])JsonConvert.DeserializeObject(responseAsString, typeof(MergeRequestDto[]));
            return requests;
        }

        public async Task<MergeRequestDetailsDto> GetMergeRequestDetailsAsync(long projectId, long mergeRequestInternalId)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/changes";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var changes = (MergeRequestDetailsDto)JsonConvert.DeserializeObject(responseAsString, typeof(MergeRequestDetailsDto));
            return changes;
        }

        public async Task<FileDto> GetFileAsync(long projectId, string branch, string path)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/repository/files/{HttpUtility.UrlEncode(path)}?ref={branch}";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var file = (FileDto)JsonConvert.DeserializeObject(responseAsString, typeof(FileDto));
            return file;
        }

        public async Task<BlobDto> GetFileBlobAsync(long projectId, string blobId)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/repository/blobs/{blobId}";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var blob = (BlobDto)JsonConvert.DeserializeObject(responseAsString, typeof(BlobDto));
            return blob;
        }

        public async Task<DiscussionDto[]> GetDiscussionsAsync(long projectId, long mergeRequestInternalId)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/discussions?per_page=100";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var discussions = (DiscussionDto[])JsonConvert.DeserializeObject(responseAsString, typeof(DiscussionDto[]));
            return discussions;
        }

        public async Task<NoteDto> AddNote(long projectId, long mergeRequestInternalId, string discussionId, string body)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/discussions/{discussionId}/notes?body={body}";
            var response = await client.PostAsync(uri, new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            var responseAsString = await response.Content.ReadAsStringAsync();
            var note = (NoteDto)JsonConvert.DeserializeObject(responseAsString, typeof(NoteDto));
            return note;
        }

        public async Task<DiscussionDto> AddDiscussion(long projectId, long mergeRequestInternalId, CreateDiscussionDto createDiscussionDto, string body)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/merge_requests/{mergeRequestInternalId}/discussions?body={body}";
            var content = JsonConvert.SerializeObject(createDiscussionDto, this.settings);
            var response = await client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
            var responseAsString = await response.Content.ReadAsStringAsync();
            var discussion = (DiscussionDto)JsonConvert.DeserializeObject(responseAsString, typeof(DiscussionDto));
            return discussion;
        }
    }
}
