using GitLabCodeReview.ViewModels;

namespace GitLabCodeReview.DTO
{
    public class ProjectViewModel : BaseViewModel
    {
        private bool isSelected;
            
        public ProjectViewModel(long id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;
                this.SchedulePropertyChanged();
            }
        }

        public long Id { get; }
        public string Name { get; }
    }
}
