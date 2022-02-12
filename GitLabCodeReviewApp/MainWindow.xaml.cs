using System.Windows;
using GitLabCodeReview.Views;

namespace GitLabCodeReviewApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var optionsService = new AppOptionsService();
            var diffService = new AppDiffService();
            var gitLabMain = new GitLabMainWindowControl(optionsService, diffService);
            this.AppContainer.Children.Add(gitLabMain);
        }
    }
}
