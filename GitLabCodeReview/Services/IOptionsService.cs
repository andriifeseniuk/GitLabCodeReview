using GitLabCodeReview.Models;

namespace GitLabCodeReview.Services
{
    public interface IOptionsService
    {
        OptionsModel LoadOptions();
        void SaveOptions(OptionsModel options);
    }
}
