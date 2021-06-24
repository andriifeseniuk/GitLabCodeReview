using Newtonsoft.Json;
using System;

namespace GitLabCodeReview.Helpers
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string jsonText)
        {
            try
            {
                var result = (T)JsonConvert.DeserializeObject(jsonText, typeof(T));
                return result;
            }
            catch (Exception exception)
            {
                var message = $"An error occured while parsing JSON:\n{jsonText}";
                throw new JsonSerializationException(message, exception);
            }
        }
    }
}
