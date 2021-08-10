using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Enums;
using GitLabCodeReview.Extensions;
using GitLabCodeReview.Helpers;
using GitLabCodeReview.Models;
using GitLabCodeReview.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GitLabCodeReview.ViewModels
{
    public class ChangeDetailsViewModel : BaseViewModel, ITreeNode
    {
        private readonly ChangeDto change;
        private readonly MergeRequestDetailsDto mergeRequest;
        private readonly string projectName;
        private readonly GitLabService service;
        private readonly ErrorService errorService;
        private readonly IDiffService diffService;
        private readonly LinesFilterOptions[] showLinesOptions;
        private LinesFilterOptions showLinesOption = LinesFilterOptions.Changes;
        private LineViewModel[] lineViewModels = new LineViewModel[0];

        public ChangeDetailsViewModel(
            ChangeDto gitLabChange,
            MergeRequestDetailsDto gitLabMergeRequest,
            string gitLabProject,
            GitLabService service,
            ErrorService globalErrorService,
            IDiffService vsDiffService)
        {
            this.change = gitLabChange;
            this.mergeRequest = gitLabMergeRequest;
            this.projectName = gitLabProject;
            this.service = service;
            this.errorService = globalErrorService;
            this.diffService = vsDiffService;
            this.DiffCommand = new DelegateCommand(x => this.ExecuteDiff());
            this.showLinesOptions = Enum.GetValues(typeof(LinesFilterOptions)).Cast<LinesFilterOptions>().ToArray();
        }

        public ICommand DiffCommand { get; }

        public string DisplayName => null;

        public LinesFilterOptions LinesFilterOption
        {
            get
            {
                return this.showLinesOption;
            }
            set
            {
                this.showLinesOption = value;
                this.SchedulePropertyChanged();
                this.RefreshLines();
            }
        }

        public LinesFilterOptions[] ShowLinesOptions => this.showLinesOptions;

        public ObservableCollection<ITreeNode> Items { get; private set; } = new ObservableCollection<ITreeNode>();

        public void ExecuteLoadLines()
        {
            try
            {
                this.RefreshDiscussions().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.errorService.AddError(ex.ToString());
            }

        }

        private async Task RefreshDiscussions()
        {
            var discussions = await this.GetDiscussions();
            var sourceFileContent = await this.GetSourceFileContentAsync(this.change);
            var targetFileContent = await this.GetTargetFileContentAsync(this.change);
            var sourceFileLines = this.GetLines(sourceFileContent);
            var targetFileLines = this.GetLines(targetFileContent);

            var linesCanBeCommented = new List<LineViewModel>();
            if (change.IsDeletedFile)
            {
                this.lineViewModels = this.GetLineViewModelsFromSource(targetFileLines);
            }
            else if (change.IsNewFile)
            {
                this.lineViewModels = this.GetLineViewModelsFromTarget(sourceFileLines);
            }
            else
            {
                Gap[] gaps;
                var hunks = HunkHelper.ParseHunks(this.change.Diff, out gaps);
                var lineViewModelsFromHunks = this.GetLineViewModelsFromHunks(hunks);
                var lineViewModelsFromGaps = this.GetLineViewModelsFromGaps(gaps, sourceFileLines);
                this.lineViewModels = lineViewModelsFromHunks.Concat(lineViewModelsFromGaps).ToArray();
            }

            foreach (var diss in discussions)
            {
                var firstNote = diss.Notes.First();
                if (firstNote.Position == null)
                {
                    continue;
                }

                if (firstNote.Position.OldPath != change.OldPath && firstNote.Position.NewPath != change.NewPath)
                {
                    continue;
                }

                var dissViewModel = new DiscussionViewModel(diss, this.service);
                foreach (var noteDto in diss.Notes)
                {
                    var noteViewModel = new NoteViewModel(noteDto);
                    dissViewModel.Details.Notes.Add(noteViewModel);
                }

                var correspondentLine = firstNote.Position.NewLine != null
                    ? this.lineViewModels.First(line => line.NumberInSourceFile == firstNote.Position.NewLine.Value)
                    : this.lineViewModels.First(line => line.NumberInTargetFile == firstNote.Position.OldLine.Value);
                correspondentLine.Details.Discussions.Add(dissViewModel);
            }

            this.RefreshLines();
        }

        private async void ExecuteDiff()
        {
            try
            {
                string targetFileName = string.Empty;
                string sourceFileName = string.Empty;

                string sourceFileExtension = string.Empty;
                string targetFileExtension = string.Empty;

                string sourceFileShortName = string.Empty;
                string targetFileShortName = string.Empty;

                string sourceHash = string.Empty;
                string targetHash = string.Empty;

                bool needLoadSource;
                bool needLoadTarget;

                var workingDirectory = this.service.GitOptions.GetWorkingOrTempDirectory();
                var dir = Path.Combine(workingDirectory, $"{DirectoryHelper.MergeRequestDirectoryName}{mergeRequest.Id}");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (change.IsNewFile)
                {
                    var path = change.NewPath;
                    needLoadSource = true;
                    needLoadTarget = false;

                    sourceFileShortName = Path.GetFileNameWithoutExtension(path);
                    targetFileShortName = sourceFileShortName;

                    sourceFileExtension = Path.GetExtension(path);
                    targetFileExtension = sourceFileExtension;

                    sourceHash = this.GetFileHash(mergeRequest.SourceBranch, path, mergeRequest);
                    targetHash = this.GetFileHash(mergeRequest.TargetBranch, path, mergeRequest);
                }
                else if (change.IsDeletedFile)
                {
                    var path = change.OldPath;
                    needLoadSource = false;
                    needLoadTarget = true;

                    targetFileShortName = Path.GetFileNameWithoutExtension(path);
                    sourceFileShortName = targetFileShortName;

                    targetFileExtension = Path.GetExtension(path);
                    sourceFileExtension = targetFileExtension;

                    sourceHash = this.GetFileHash(mergeRequest.SourceBranch, path, mergeRequest);
                    targetHash = this.GetFileHash(mergeRequest.TargetBranch, path, mergeRequest);
                }
                else
                {
                    needLoadSource = true;
                    needLoadTarget = true;

                    sourceFileShortName = Path.GetFileNameWithoutExtension(change.NewPath);
                    targetFileShortName = Path.GetFileNameWithoutExtension(change.OldPath);

                    sourceFileExtension = Path.GetExtension(change.NewPath);
                    targetFileExtension = Path.GetExtension(change.OldPath);

                    sourceHash = this.GetFileHash(mergeRequest.SourceBranch, change.NewPath, mergeRequest);
                    targetHash = this.GetFileHash(mergeRequest.TargetBranch, change.OldPath, mergeRequest);
                }

                sourceFileName = $"{sourceFileShortName}_{sourceHash}_source{sourceFileExtension}";
                targetFileName = $"{targetFileShortName}_{targetHash}_target{targetFileExtension}";

                var sourceFileLocalPath = Path.Combine(dir, sourceFileName);
                var targetFileLocaPath = Path.Combine(dir, targetFileName);

                var sourceFileContent = string.Empty;
                var targetFileContent = string.Empty;

                string sourceFileLocalRepositoryPath = null;
                if (!string.IsNullOrWhiteSpace(this.service.GitOptions.RepositoryLocalPath) && !change.IsDeletedFile)
                {
                    sourceFileLocalRepositoryPath = Path.Combine(
                        this.service.GitOptions.RepositoryLocalPath,
                        this.projectName,
                        change.NewPath.Replace('/', '\\'));
                }

                if (!string.IsNullOrWhiteSpace(sourceFileLocalRepositoryPath)
                    && File.Exists(sourceFileLocalRepositoryPath))
                {
                    sourceFileLocalPath = sourceFileLocalRepositoryPath;
                }
                else if (!File.Exists(sourceFileLocalPath))
                {
                    if (needLoadSource)
                    {
                        sourceFileContent = await this.service.GetFileContentAsync(mergeRequest.SourceBranch, change.NewPath);
                    }

                    File.WriteAllText(sourceFileLocalPath, sourceFileContent);
                }

                if (!File.Exists(targetFileLocaPath))
                {
                    if (needLoadTarget)
                    {
                        targetFileContent = await this.service.GetFileContentAsync(mergeRequest.TargetBranch, change.OldPath);
                    }

                    File.WriteAllText(targetFileLocaPath, targetFileContent);
                }

                this.diffService.Diff(targetFileLocaPath, sourceFileLocalPath);
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

        private IEnumerable<LineViewModel> GetLineViewModelsFromHunks(Hunk[] hunks)
        {
            var lines = new List<LineViewModel>();
            foreach (var hunk in hunks)
            {
                foreach (var hunkLine in hunk.Lines)
                {
                    var details = new LineDetailsViewModel(
                        hunkLine.NumberInSourceFile,
                        hunkLine.NumberInTargetFile,
                        this.mergeRequest,
                        this.change,
                        this.service);

                    var lineVm = new LineViewModel(
                        hunkLine.NumberInChanges,
                        hunkLine.NumberInSourceFile,
                        hunkLine.NumberInTargetFile,
                        hunkLine.Text,
                        details);
                    lines.Add(lineVm);
                }
            }

            return lines;
        }

        private IEnumerable<LineViewModel> GetLineViewModelsFromGaps(Gap[] gaps, string[] sourceLines)
        {
            var viewModelsFromGaps = new List<LineViewModel>();
            foreach (var gap in gaps)
            {
                for (var i = 0; i < gap.Length; i++)
                {
                    var numberInSourceFile = gap.StartInSourceFile + i;
                    var numberInTargetFile = gap.StartInTargetFile + i;
                    var numberInChanges = gap.StartInChanges + i;
                    var indexInSourceFile = numberInSourceFile - 1;
                    var line = sourceLines[indexInSourceFile];
                    var lineVm = new LineViewModel(numberInChanges, numberInSourceFile, numberInTargetFile, line);
                    viewModelsFromGaps.Add(lineVm);
                }
            }

            return viewModelsFromGaps;
        }

        private string[] GetLines(string fileContent)
        {
            var lines = fileContent == null ? new string[0] : fileContent.Split('\n');
            return lines;
        }

        private LineViewModel[] GetLineViewModelsFromSource(IEnumerable<string> sourceLines)
        {
            var lineViewModelsFromSource = sourceLines
                .Select((line, i) => new LineViewModel(i + 1, i + 1, null, line, new LineDetailsViewModel(i, null, this.mergeRequest, this.change, this.service)))
                .ToArray();
            return lineViewModelsFromSource;
        }

        private LineViewModel[] GetLineViewModelsFromTarget(IEnumerable<string> targetLines)
        {
            lineViewModels = targetLines
                .Select((line, i) => new LineViewModel(i + 1, null, i + 1, line, new LineDetailsViewModel(null, i, this.mergeRequest, this.change, this.service)))
                .ToArray();
            return lineViewModels;
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

        private void RefreshLines()
        {
            this.Items.Clear();
            var filteredLines = this.GetFilteredLines();
            if (filteredLines.Any())
            {
                foreach (var line in filteredLines)
                {
                    this.Items.Add(line);
                }

                var expandedState = this.LinesFilterOption == LinesFilterOptions.Discussions
                    || this.LinesFilterOption == LinesFilterOptions.Changes;
                foreach (var line in filteredLines)
                {
                    if (line.Details != null && line.Details.Discussions.Any())
                    {
                        line.IsExpanded = expandedState;
                        line.Details.IsExpanded = expandedState;
                        foreach (var diss in line.Details.Discussions)
                        {
                            diss.IsExpanded = expandedState;
                        }
                    }
                }
            }
        }

        private IEnumerable<LineViewModel> GetFilteredLines()
        {
            switch (this.LinesFilterOption)
            {
                case LinesFilterOptions.All:
                    return this.lineViewModels
                        .OrderBy(l => l.NumberInChanges)
                        .ToArray();

                case LinesFilterOptions.Source:
                    return this.lineViewModels
                        .Where(l => l.NumberInSourceFile != null)
                        .OrderBy(l => l.NumberInChanges)
                        .ToArray();

                case LinesFilterOptions.Target:
                    return this.lineViewModels
                        .Where(l => l.NumberInTargetFile != null)
                        .OrderBy(l => l.NumberInChanges)
                        .ToArray();

                case LinesFilterOptions.Discussions:
                    var linesWithDiscussions = this.lineViewModels
                        .Where(l => l.Details != null && l.Details.Discussions.Any())
                        .OrderBy(l => l.NumberInChanges)
                        .ToArray();
                    return linesWithDiscussions;

                case LinesFilterOptions.Changes:
                    var linesWithDetails = this.lineViewModels
                        .Where(l => l.Details != null)
                        .OrderBy(l => l.NumberInChanges)
                        .ToArray();
                    return linesWithDetails;

                default:
                    throw new ArgumentOutOfRangeException($"Option {this.LinesFilterOption} is out of range.");
            }
        }

        private string GetFileHash(string branch, string path, MergeRequestDetailsDto request)
        {
            var hash = $"{branch}/{path}-sha{request.DiffRefs.HeadSha}".GetHashCode().ToString("X8");
            return hash;
        }
    }
}
