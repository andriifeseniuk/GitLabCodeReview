using EnvDTE;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Enums;
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
        private readonly GitLabService service;
        private readonly ErrorService errorService;
        private readonly LinesFilterOptions[] showLinesOptions;
        private LinesFilterOptions showLinesOption = LinesFilterOptions.Changes;
        private LineViewModel[] lineViewModels = new LineViewModel[0];

        public ChangeDetailsViewModel(
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
                foreach(var noteDto in diss.Notes)
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
            }
        }

        private IEnumerable<LineViewModel> GetFilteredLines()
        {
            switch(this.LinesFilterOption)
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
    }
}
