//------------------------------------------------------------------------------
// <copyright file="GitLabMainWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace GitLabCodeReview.Views
{
    using Services;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GitLabMainWindowControl.
    /// </summary>
    public partial class GitLabMainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabMainWindowControl"/> class.
        /// </summary>
        public GitLabMainWindowControl(IOptionsService optionsService, IDiffService vsDiffService)
        {
            this.InitializeComponent();
            var mainViewModel = new MainViewModel(optionsService, vsDiffService);
            this.DataContext = mainViewModel;
        }

        private void MyToolWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var mainViewModel = (MainViewModel)this.DataContext;
            mainViewModel.RefreshAll();
        }
    }    
}