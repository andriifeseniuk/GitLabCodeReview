using GitLabCodeReview.DTO;

namespace GitLabCodeReview.ViewModels
{
    public class NoteViewModel : BaseViewModel
    {
        public NoteViewModel(NoteDto note)
        {
            this.Note = note;
        }

        public NoteDto Note { get; private set; }
    }
}
