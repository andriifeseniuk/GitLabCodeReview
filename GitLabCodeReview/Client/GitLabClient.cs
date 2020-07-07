using GitLabCodeReview.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GitLabCodeReview.Client
{
    public class GitLabClient : IDisposable
    {
        private readonly string apiUrl;
        private readonly HttpClient client;

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
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task<GitLabUser> GetUserAsync()
        {
            var uri = $"{this.apiUrl}/user";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var user = (GitLabUser)JsonConvert.DeserializeObject(responseAsString, typeof(GitLabUser));
            return user;
        }

        public async Task<IEnumerable<GitLabProject>> GetProjects(long userId)
        {
            var uri = $"{this.apiUrl}/users/{userId}/projects";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var projects = (GitLabProject[])JsonConvert.DeserializeObject(responseAsString, typeof(GitLabProject[]));
            return projects;
        }

        public async Task<IEnumerable<GitLabMergeRequest>> GetMergeRequests(long projectId)
        {
            var uri = $"{this.apiUrl}/projects/{projectId}/merge_requests?state=opened";
            var response = await client.GetAsync(uri);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var requests = (GitLabMergeRequest[])JsonConvert.DeserializeObject(responseAsString, typeof(GitLabMergeRequest[]));
            return requests;
        }
    }
}
