using System.Collections.ObjectModel;

namespace GitLabCodeReview.Services
{
    public class ErrorService
    {
        public ObservableCollection<string> Errors { get; } = new ObservableCollection<string>();

        public void AddError(string message)
        {
            this.Errors.Add(message);
        }
    }
}
