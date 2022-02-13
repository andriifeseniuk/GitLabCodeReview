using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace GitLabCodeReview.Views
{
    /// <summary>
    /// Interaction logic for HorizontalListBox.xaml
    /// </summary>
    public partial class HorizontalListBox : UserControl
    {
        public HorizontalListBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(HorizontalListBox.ItemsSource), typeof(IEnumerable), typeof(HorizontalListBox));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(HorizontalListBox.SelectedItem), typeof(object), typeof(HorizontalListBox));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
    }
}
