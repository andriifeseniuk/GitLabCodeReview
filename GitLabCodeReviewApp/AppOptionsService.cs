using GitLabCodeReview.Services;
using GitLabCodeReview.Models;
using System.Configuration;

namespace GitLabCodeReviewApp
{
    public class AppOptionsService : IOptionsService
    {
        public OptionsModel LoadOptions()
        {
            var options = new OptionsModel();
            options.ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
            options.PrivateToken = ConfigurationManager.AppSettings["PrivateToken"];
            return options;
        }

        public void SaveOptions(OptionsModel options)
        {
        }
    }
}
