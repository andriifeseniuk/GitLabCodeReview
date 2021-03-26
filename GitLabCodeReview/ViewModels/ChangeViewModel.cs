﻿using GitLabCodeReview.DTO;
using GitLabCodeReview.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace GitLabCodeReview.ViewModels
{
    public class ChangeViewModel : BaseViewModel, IParentTreeNode
    {
        private readonly ChangeDto change;
        private readonly GitLabService service;
        private bool isExpanded;

        public ChangeViewModel(
            ChangeDto gitLabChange,
            MergeRequestDetailsDto gitLabMergeRequest,
            GitLabService service,
            ErrorService globalErrorService)
        {
            this.change = gitLabChange;
            this.service = service;
            var details = new ChangeDetailsViewModel(gitLabChange, gitLabMergeRequest, service, globalErrorService);
            this.Items.Add(details);
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
    }
}
