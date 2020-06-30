using System;

namespace GitLabCodeReview.Common.Commands
{
    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action<object> execute)
            : base(obj => execute(obj))
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
            : base(obj => execute(obj), obj => canExecute(obj))
        {
        }
    }
}
