//------------------------------------------------------------------------------
// <copyright file="GitLabMainWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace GitLabCodeReview.Views
{
    using System;
    using System.Windows.Controls;
    using Microsoft.VisualStudio.Shell;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GitLabMainWindowControl.
    /// </summary>
    public partial class GitLabMainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabMainWindowControl"/> class.
        /// </summary>
        public GitLabMainWindowControl()
        {
            this.InitializeComponent();
            var mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;
        }

        internal void SetPackage(Package package)
        {
            var mainViewModel = (MainViewModel)this.DataContext;
            mainViewModel.SetPackge(package);
        }
    }
}