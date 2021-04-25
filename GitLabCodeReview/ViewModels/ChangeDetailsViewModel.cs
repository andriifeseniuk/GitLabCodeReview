using EnvDTE;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Enums;
using GitLabCodeReview.Helpers;
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
        private LinesFilterOptions showLinesOption = LinesFilterOptions.Discussions;
        private LineViewModel[] sourceFileLines = new LineViewModel[0];
        private LineViewModel[] targetFileLines = new LineViewModel[0];
        private bool isExpanded;

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
            var sourceFileContent = await this.GetSourceFileContentAsync(this.change);
            var targetFileContent = await this.GetTargetFileContentAsync(this.change);

            this.sourceFileLines = this.GetLineViewModels(sourceFileContent, true).ToArray();
            this.targetFileLines = this.GetLineViewModels(targetFileContent, false).ToArray();

            var linesCanBeCommented = new List<LineViewModel>();
            if (change.IsDeletedFile)
            {
                foreach(var lineVm in this.targetFileLines)
                {
                    linesCanBeCommented.Add(lineVm);
                    lineVm.IsRemoved = true;
                }
            }
            else if (change.IsNewFile)
            {
                foreach(var lineVm in this.sourceFileLines)
                {
                    linesCanBeCommented.Add(lineVm);
                    lineVm.IsAdded = true;
                }
            }
            else
            {
                var hunks = HunkHelper.ParseHunks(this.change.Diff);
                foreach (var hunk in hunks)
                {
                    for (var i = 0; i < hunk.LengthInSourceFile; i++)
                    {
                        var number = hunk.StartInSourceFile + i;
                        var lineVm = this.sourceFileLines[number - 1];
                        linesCanBeCommented.Add(lineVm);
                    }

                    for (var i = 0; i < hunk.LenghtInTargetFile; i++)
                    {
                        var number = hunk.StartInTargetFile + i;
                        var lineVm = this.targetFileLines[number - 1];
                        linesCanBeCommented.Add(lineVm);
                    }

                    foreach(var hunkLine in hunk.Lines)
                    {
                        if (hunkLine.IsLineAdded)
                        {
                            this.sourceFileLines[hunkLine.NumberInSourceFile.Value - 1].IsAdded = true;
                        }
                        else if (hunkLine.IsLineRemoved)
                        {
                            this.targetFileLines[hunkLine.NumberInTargetFile.Value - 1].IsRemoved = true;
                        }
                    }
                }
            }

            foreach(var lineVm in linesCanBeCommented)
            {
                var details = new LineDetailsViewModel(lineVm.Number, lineVm.Text, lineVm.IsSourceBranch, this.mergeRequest, this.change, this.service);
                lineVm.Details = details;
                lineVm.Items.Add(details);
            }

            var discussions = await this.GetDiscussions();
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

                if (firstNote.Position.NewLine != null)
                {
                    this.sourceFileLines[firstNote.Position.NewLine.Value - 1].Details.Discussions.Add(dissViewModel);
                }
                else
                {
                    this.targetFileLines[firstNote.Position.OldLine.Value - 1].Details.Discussions.Add(dissViewModel);
                }
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

        private IEnumerable<LineViewModel> GetLineViewModels(string fileContent, bool isSourceBranch)
        {
            var lines = fileContent == null ? new string[0] : fileContent.Split('\n');
            var viewModels = lines.Select((l, i) => new LineViewModel(i + 1, l, isSourceBranch, this.mergeRequest, this.change, this.service)).ToArray();
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
                    return this.targetFileLines.Concat(this.sourceFileLines);

                case LinesFilterOptions.Source:
                    return this.sourceFileLines;

                case LinesFilterOptions.Target:
                    return this.targetFileLines;

                case LinesFilterOptions.Discussions:
                    var linesWithDiscussions = this.targetFileLines.Where(l => l.Details != null && l.Details.Discussions.Any())
                        .Concat(this.sourceFileLines.Where(l => l.Details != null && l.Details.Discussions.Any())).ToArray();
                    return linesWithDiscussions;

                default:
                    throw new ArgumentOutOfRangeException($"Option {this.LinesFilterOption} is out of range.");
            }
        }
    }
}
