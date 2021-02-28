using GitLabCodeReview.DTO;

namespace GitLabCodeReview.ViewModels
{
    public class NoteViewModel : BaseViewModel, ITreeNode
    {
        public NoteViewModel(NoteDto note)
        {
            this.Note = note;
        }

        public string DisplayName
        {
            get
            {
                return this.Note.Body;
            }
        }

        public NoteDto Note { get; private set; }
    }
}
