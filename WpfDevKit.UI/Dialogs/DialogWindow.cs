using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Represents the dialog window used to display modal dialogs in WPF.
    /// Automatically centers itself over its owner window when initialized.
    /// </summary>
    [DebuggerStepThrough]
    public partial class DialogWindow : IDialogWindow
    {
        /// <summary>
        /// Handles the SourceInitialized event of the window.
        /// Repositions the dialog window to be centered over its owner window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            if (Owner.WindowState == WindowState.Maximized)
            {
                if (!GetWindowRect(new WindowInteropHelper(Owner).Handle, out RECT rect))
                    return;
                Left = rect.Left + ((rect.Right - rect.Left - ActualWidth) / 2);
                Top = rect.Top + ((rect.Bottom - rect.Top - ActualHeight) / 2);
            }
            else
            {
                Left = Owner.Left + ((Owner.ActualWidth - ActualWidth) / 2);
                Top = Owner.Top + ((Owner.ActualHeight - ActualHeight) / 2);
            }
        }

        /// <summary>
        /// Defines the RECT structure used to store window rectangle coordinates.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            /// <summary>
            /// Gets or sets the left position of the dialog window.
            /// </summary>
            public int Left;

            /// <summary>
            /// Gets or sets the top position of the dialog window.
            /// </summary>
            public int Top;

            /// <summary>
            /// Gets or sets the right position of the dialog window.
            /// </summary>
            public int Right;

            /// <summary>
            /// Gets or sets the bottom position of the dialog window.
            /// </summary>
            public int Bottom;
        }

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window.
        /// </summary>
        /// <param name="hWnd">The handle to the window.</param>
        /// <param name="lpRect">The RECT structure to receive the coordinates.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    }
}
