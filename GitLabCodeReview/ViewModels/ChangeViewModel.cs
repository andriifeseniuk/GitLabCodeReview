using EnvDTE;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Enums;
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
    public class ChangeViewModel : BaseViewModel, IParentTreeNode
    {
        private const string MoreText = "more";
        private const string LessText = "less";
        private readonly ChangeDto change;
        private readonly MergeRequestDetailsDto mergeRequest;
        private readonly GitLabService service;
        private readonly ErrorService errorService;
        private readonly LinesFilterOptions[] showLinesOptions;
        private bool isMoreSectionVisible = false;
        private string moreLessText = MoreText;
        private LinesFilterOptions showLinesOption = LinesFilterOptions.Discussions;
        private LineViewModel[] sourceFileLines;
        private LineViewModel[] targetFileLines;
        private bool isExpanded;
        private DummyTreeNode dummyTreeNode = new DummyTreeNode { DisplayName = "No lines to show" };

        public ChangeViewModel(
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
            this.LoadLinesCommand = new DelegateCommand(x => this.ExecuteLoadLines());
            this.showLinesOptions = Enum.GetValues(typeof(LinesFilterOptions)).Cast<LinesFilterOptions>().ToArray();
            this.Items.Add(this.dummyTreeNode);
        }

        public ICommand DiffCommand { get; }

        public ICommand LoadLinesCommand { get; }

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

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                this.isExpanded = value;
                this.SchedulePropertyChanged();
            }
        }

        public ObservableCollection<ITreeNode> Items { get; private set; } = new ObservableCollection<ITreeNode>();

        private void ExecuteLoadLines()
        {
            this.RefreshDiscussions().ConfigureAwait(false);
        }

        private async Task RefreshDiscussions()
        {
            var sourceFileContent = await this.GetSourceFileContentAsync(this.change);
            var targetFileContent = await this.GetTargetFileContentAsync(this.change);

            this.sourceFileLines = this.GetLineViewModels(sourceFileContent, true).ToArray();
            this.targetFileLines = this.GetLineViewModels(targetFileContent, false).ToArray();

            var discussions = await this.GetDiscussions();
            foreach (var diss in discussions)
            {
                var dissViewModel = new DiscussionViewModel(diss);
                foreach(var noteDto in diss.Notes)
                {
                    var noteViewModel = new NoteViewModel(noteDto);
                    dissViewModel.Notes.Add(noteViewModel);
                }

                var firstNote = diss.Notes.First();
                if (firstNote.Position.NewLine != null)
                {
                    this.sourceFileLines[firstNote.Position.NewLine.Value].Discussions.Add(dissViewModel);
                }
                else
                {
                    this.targetFileLines[firstNote.Position.OldLine.Value].Discussions.Add(dissViewModel);
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
            else
            {
                Items.Add(this.dummyTreeNode);
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
                    var linesWithDiscussions = this.targetFileLines.Where(l => l.Discussions.Any())
                        .Concat(this.sourceFileLines.Where(l => l.Discussions.Any())).ToArray();
                    return linesWithDiscussions;

                default:
                    throw new ArgumentOutOfRangeException($"Option {this.LinesFilterOption} is out of range.");
            }
        }
    }
}
