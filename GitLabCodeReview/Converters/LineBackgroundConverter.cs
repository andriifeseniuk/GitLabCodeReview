using GitLabCodeReview.Helpers;
using GitLabCodeReview.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitLabCodeReview.Converters
{
    public class LineBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var lineVm = value as LineViewModel;
            if (lineVm == null)
            {
                return Brushes.Transparent;
            }

            if (lineVm.IsAdded)
            {
                var addedBrush = Brushes.LightGreen;
                return addedBrush;
            }

            if (lineVm.IsRemoved)
            {
                var lightRed = Color.FromRgb(255, 100, 50);
                var removedBrush = new SolidColorBrush(lightRed);
                return removedBrush;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
