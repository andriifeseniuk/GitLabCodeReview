namespace GitLabCodeReview.Extensions
{
    public static class UriExtensions
    {

        public static string Parameter(this string uri, string parameterName, string value)
        {
            if (uri.Contains("?"))
            {
                return $"{uri}&{parameterName}={value}";
            }

            return $"{uri}?{parameterName}={value}";
        }
    }
}
