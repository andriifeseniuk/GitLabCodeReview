namespace GitLabCodeReview.Services
{
    public interface IDiffService
    {
        void Diff(string targetFileLocaPath, string sourceFileLocalPath);
    }
}
