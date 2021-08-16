using GitLabCodeReview.Views;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace GitLabCodeReviewVsix2019
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("a073dcd4-a341-46d8-b754-21020e926cd9")]
    public class GitLabMainWindow2019 : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabMainWindow2019"/> class.
        /// </summary>
        public GitLabMainWindow2019() : base(null)
        {
            this.Caption = "GitLab Code Review";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new GitLabMainWindowControl(new OptionsService(), new DiffService());
        }
    }
}
