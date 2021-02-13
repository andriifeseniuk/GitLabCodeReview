﻿using GitLabCodeReview.DTO;
using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public class DiscussionViewModel : BaseViewModel
    {
        public DiscussionViewModel(DiscussionDto gitLabDiscussion)
        {
            this.Discussion = gitLabDiscussion;
        }

        public DiscussionDto Discussion { get; }

        public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();
    }
}