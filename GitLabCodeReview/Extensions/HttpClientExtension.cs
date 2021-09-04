using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GitLabCodeReview.Extensions
{
    public static class HttpClientExtension
    {
        public static async Task<HttpResponseMessage> GetAndValidateAsync(this HttpClient client, string uri)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var response = await client.GetAsync(uri);
            response.Validate(uri);
            return response;
        }

        public static async Task<HttpResponseMessage> PostAndValidateAsync(this HttpClient client, string uri, HttpContent content)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var response = await client.PostAsync(uri, content);
            response.Validate(uri);
            return response;
        }

        public static bool Validate(this HttpResponseMessage response, string uri)
        {
            if (!response.IsSuccessStatusCode)
            {
                var tokenIndex = uri.IndexOf("private_token");
                var safeUri = tokenIndex < 0 ? uri : uri.Substring(0, tokenIndex);
                throw new InvalidOperationException($"{(int)response.StatusCode} - {response.ReasonPhrase} - {safeUri}");
            }

            return true;
        }
    }
}
