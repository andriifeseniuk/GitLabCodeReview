using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GitLabCodeReview.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SchedulePropertyChanged([CallerMemberName] string propertyName = null)
        {
            Application.Current.Dispatcher.InvokeAsync(() => this.OnPropertyChanged(propertyName));
        }
    }
}
