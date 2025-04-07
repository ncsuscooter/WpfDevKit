using System.Threading.Tasks;

namespace WpfDevKit.UI.Core
{
    public interface IConfirmNavigation
    {
        /// <summary>
        /// Called before the navigation system switches away from this page.
        /// Allows the page to cancel or delay navigation.
        /// </summary>
        /// <returns><c>true</c> to allow navigation; otherwise, <c>false</c>.</returns>
        Task<bool> CanNavigateAwayAsync();

        /// <summary>
        /// Called before the navigation system switches away from this page.
        /// Allows the page to cancel or delay navigation.
        /// </summary>
        /// <returns><c>true</c> to allow navigation; otherwise, <c>false</c>.</returns>
        bool CanNavigateAway();
    }
}
