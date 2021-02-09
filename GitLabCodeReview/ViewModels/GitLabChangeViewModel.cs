using EnvDTE;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitLabCodeReview.ViewModels
{
    public class GitLabChangeViewModel : BaseViewModel, ITreeNode
    {
        private const string MoreText = "more";
        private const string LessText = "less";
        private readonly ChangeDto change;
        private readonly MergeRequestDetailsDto mergeRequest;
        private readonly GitLabService service;
        private readonly ErrorService errorService;
        private Visibility moreSectionVisibility = Visibility.Collapsed;
        private string moreLessText = MoreText;

        public GitLabChangeViewModel(
            ChangeDto gitLabChange,
            MergeRequestDetailsDto gitLabMergeRequest,
            GitLabService service,
            ErrorService globalErrorService)
        {
            this.change = gitLabChange;
            this.mergeRequest = gitLabMergeRequest;
            this.service = service;
            this.errorService = globalErrorService;
            this.DiffCommand = new DelegateCommand(x => this.ExecuteDiff());
            this.MoreLessCommand = new DelegateCommand(x => this.ExecuteMoreLess());
        }

        public ICommand DiffCommand { get; }

        public ICommand MoreLessCommand { get; }

        public Visibility MoreSectionVisibility
        {
            get
            {
                return this.moreSectionVisibility;
            }
            set
            {
                this.moreSectionVisibility = value;
                this.SchedulePropertyChanged();
            }
        }

        public string MoreLessText
        {
            get
            {
                return this.moreLessText;
            }
            set
            {
                this.moreLessText = value;
                this.SchedulePropertyChanged();
            }
        }

        public string DisplayName
        {
            get
            {
                if (this.change.IsDeletedFile)
                {
                    return Path.GetFileName(this.change.OldPath);
                }

                if (this.change.IsRenamedFile)
                {
                    return $"{Path.GetFileName(this.change.NewPath)}[{Path.GetFileName(this.change.OldPath)}]";
                }

                return Path.GetFileName(this.change.NewPath);
            }
        }

        public string FullPath
        {
            get
            {
                if (this.change.IsDeletedFile)
                {
                    return this.change.OldPath;
                }

                return this.change.NewPath;
            }
        }

        public ObservableCollection<GitLabDiscussionViewModel> Discussions { get; } = new ObservableCollection<GitLabDiscussionViewModel>();

        private void ExecuteMoreLess()
        {
            if (this.MoreSectionVisibility == Visibility.Visible)
            {
                this.MoreSectionVisibility = Visibility.Collapsed;
                this.MoreLessText = MoreText;
            }
            else
            {
                this.MoreSectionVisibility = Visibility.Visible;
                this.MoreLessText = LessText;
                this.RefreshDiscussions().ConfigureAwait(false);
            }
        }

        private async Task RefreshDiscussions()
        {
            var sourceFileContent = await this.GetSourceFileContentAsync(this.change);
            var targetFileContent = await this.GetTargetFileContentAsync(this.change);

            var sourceFileLines = this.GetLineViewModels(sourceFileContent, true);
            var targetFileLines = this.GetLineViewModels(targetFileContent, false);

            // TODO join lines and discussions
            this.Discussions.Clear();
            var discussions = await this.GetDiscussions();
            foreach(var discussion in discussions)
            {
                this.Discussions.Add(new GitLabDiscussionViewModel(
                    discussion,
                    sourceFileLines.Select(l => l.Text).ToArray(),
                    targetFileLines.Select(l => l.Text).ToArray()));
            }
        }

        private async void ExecuteDiff()
        {
            try
            {
                if (change.IsNewFile)
                {
                    // TODO
                    return;
                }

                if (change.IsDeletedFile)
                {
                    // TODO
                    return;
                }

                if (change.IsRenamedFile)
                {
                    // TODO
                    return;
                }

                var sourceFileContent = await this.service.GetFileContentAsync(mergeRequest.SourceBranch, change.NewPath);
                var targetFileContent = await this.service.GetFileContentAsync(mergeRequest.TargetBranch, change.NewPath);

                var dir = Path.Combine(this.service.GitOptions.WorkingDirectory, $"MergeRequest{mergeRequest.Id}");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var changeDir = Path.GetDirectoryName(change.NewPath);
                var fileName = Path.GetFileNameWithoutExtension(change.NewPath);
                var extension = Path.GetExtension(change.NewPath);

                var sourceFileName = $"{fileName}_source_{extension}";
                var targetFileName = $"{fileName}_target_{extension}";

                var sourceFileLocalPath = Path.Combine(dir, sourceFileName);
                var sourceFileLocalDir = Path.GetDirectoryName(sourceFileLocalPath);
                if (!Directory.Exists(sourceFileLocalDir))
                {
                    Directory.CreateDirectory(sourceFileLocalDir);
                }

                File.WriteAllText(sourceFileLocalPath, sourceFileContent);

                var targetFileLocaPath = Path.Combine(dir, targetFileName);
                var targetFileLocalDir = Path.GetDirectoryName(targetFileLocaPath);
                if (!Directory.Exists(targetFileLocalDir))
                {
                    Directory.CreateDirectory(targetFileLocalDir);
                }

                File.WriteAllText(targetFileLocaPath, targetFileContent);

                var serviceProvoder = GitLabMainWindowCommand.Instance.ServiceProvider;
                var dte = (DTE)serviceProvoder.GetService(typeof(DTE));
                var arg = $"\"{targetFileLocaPath}\" \"{sourceFileLocalPath}\"";
                dte.ExecuteCommand("Tools.DiffFiles", arg);
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }
        }

        private async Task<DiscussionDto[]> GetDiscussions()
        {
            var discussions = await service.GetDiscussionsAsync();
            return discussions;
        }

        private IEnumerable<LineViewModel> GetLineViewModels(string fileContent, bool isSourceBranch)
        {
            var lines = fileContent == null ? new string[0] : fileContent.Split('\n');
            var viewModels = lines.Select((l, i) => new LineViewModel(i, l, isSourceBranch)).ToArray();
            return viewModels;
        }

        private async Task<string> GetSourceFileContentAsync(ChangeDto change)
        {
            if (change.IsDeletedFile)
            {
                return null;
            }

            var sourceFileContent = await this.service.GetFileContentAsync(this.mergeRequest.SourceBranch, change.NewPath);
            return sourceFileContent;
        }


        private async Task<string> GetTargetFileContentAsync(ChangeDto change)
        {
            if (change.IsNewFile)
            {
                return null;
            }

            var targetFileContent = await this.service.GetFileContentAsync(this.mergeRequest.TargetBranch, change.OldPath);
            return targetFileContent;
        }

    }
}
