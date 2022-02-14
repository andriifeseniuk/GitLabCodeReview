using GitLabCodeReview.Services;
using GitLabCodeReview.Models;
using System.Configuration;
using System;
using System.Text;

namespace GitLabCodeReviewApp
{
    public class AppOptionsService : IOptionsService
    {
        public OptionsModel LoadOptions()
        {
            var options = new OptionsModel();
            options.ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
            var nekot = ConfigurationManager.AppSettings["Nekot"];
            var data = Convert.FromBase64String(nekot);
            options.PrivateToken = Encoding.UTF8.GetString(data);
            return options;
        }

        public void SaveOptions(OptionsModel options)
        {
        }
    }
}
