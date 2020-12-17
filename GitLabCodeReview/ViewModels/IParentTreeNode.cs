using System.Collections.ObjectModel;

namespace GitLabCodeReview.ViewModels
{
    public interface IParentTreeNode : ITreeNode
    {
        ObservableCollection<ITreeNode> Items { get; }
        bool IsExpanded { get; set; }
    }
}
