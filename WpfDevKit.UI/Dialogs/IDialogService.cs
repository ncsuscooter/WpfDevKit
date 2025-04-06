using System;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Defines a service for displaying dialogs and receiving user feedback.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays a dialog window with the data context object provided.
        /// </summary>
        /// <param name="dialogContext">The dialog context to display.</param>
        void ShowDialog(IDialogContext dialogContext);

        /// <summary>
        /// Displays a dialog window with error details and logs the error.
        /// </summary>
        /// <param name="exception">The exception to log and display.</param>
        /// <param name="type">The type where the error occurred.</param>
        /// <param name="fileName">The source file where the error occurred.</param>
        /// <param name="memberName">The member name where the error occurred.</param>
        void ShowDialog(Exception exception, Type type = null, string fileName = null, string memberName = null);

        /// <summary>
        /// Displays a dialog window with the specified message, title, image, and buttons.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="title">The title of the dialog window.</param>
        /// <param name="image">The image to display in the dialog.</param>
        /// <param name="buttons">The buttons to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, string title, TDialogImage image, TDialogButtons buttons);

        /// <summary>
        /// Displays a dialog with message, title, image and OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="title">The title of the dialog window.</param>
        /// <param name="image">The image to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, string title, TDialogImage image);

        /// <summary>
        /// Displays a dialog with message, image and OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="image">The image to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, TDialogImage image);

        /// <summary>
        /// Displays a dialog with message and title and OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="title">The title of the dialog window.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, string title);

        /// <summary>
        /// Displays a dialog with message and OK button.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message);

        /// <summary>
        /// Displays a dialog with message, image and buttons.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="image">The image to display in the dialog.</param>
        /// <param name="buttons">The buttons to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, TDialogImage image, TDialogButtons buttons);

        /// <summary>
        /// Displays a dialog with message, title and buttons.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="title">The title of the dialog window.</param>
        /// <param name="buttons">The buttons to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, string title, TDialogButtons buttons);

        /// <summary>
        /// Displays a dialog with message and buttons.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="buttons">The buttons to display in the dialog.</param>
        /// <returns>The result of the dialog interaction.</returns>
        TDialogResult ShowDialog(string message, TDialogButtons buttons);
    }
}
