using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfDevKit.UI.Interfaces;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Provides an abstract base class for commands that perform actions asynchronously.
    /// Inherits from <see cref="ObservableBase"/> and exposes a <see cref="Command{T}"/> for command execution.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class CommandBase : ObservableBase
    {
        private readonly ICommandFactory commandFactory;

        /// <summary>
        /// The default message when no action is specified for the command.
        /// </summary>
        public const string NO_ACTION_MESSAGE = "No action specified for the command provided";

        /// <summary>
        /// Gets the command that executes <see cref="DoPerformCommandAsync"/> with a string parameter.
        /// </summary>
        public ICommand Command => commandFactory.GetCommand<string>(s => DoPerformCommandAsync(s));

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class with the specified command factory.
        /// </summary>
        /// <param name="commandFactory">
        /// The <see cref="ICommandFactory"/> used to create commands.
        /// </param>
        public CommandBase(ICommandFactory commandFactory) => this.commandFactory = commandFactory;

        /// <summary>
        /// Abstract method that should be implemented to perform the asynchronous command action.
        /// </summary>
        /// <param name="commandName">The name of the command to be executed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task DoPerformCommandAsync(string commandName);
    }
}
