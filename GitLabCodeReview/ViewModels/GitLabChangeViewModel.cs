﻿using EnvDTE;
using GitLabCodeReview.Common.Commands;
using GitLabCodeReview.DTO;
using GitLabCodeReview.Enums;
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
        private LinesFilterOptions showLinesOption = LinesFilterOptions.Discussions;
        private LineViewModel[] sourceFileLines;
        private LineViewModel[] targetFileLines;

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
            }
        }

        public ObservableCollection<LineViewModel> FilteredLines { get; } = new ObservableCollection<LineViewModel>();

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

            this.sourceFileLines = this.GetLineViewModels(sourceFileContent, true).ToArray();
            this.targetFileLines = this.GetLineViewModels(targetFileContent, false).ToArray();

            var discussions = await this.GetDiscussions();
            foreach (var diss in discussions)
            {
                var dissViewModel = new GitLabDiscussionViewModel(diss);
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

            this.FilteredLines.Clear();
            var filteredLines = this.GetFilteredLines();
            foreach (var line in filteredLines)
            {
                this.FilteredLines.Add(line);
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
