using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DirectNXAML.Helpers
{
    // This code comes from https://github.com/jamesmontemagno/mvvm-helpers
    /// <summary>
	/// Interface for Async Command
	/// </summary>
	public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Execute the command async.
        /// </summary>
        /// <returns>Task to be awaited on.</returns>
        Task ExecuteAsync();

        /// <summary>
        /// Raise a CanExecute change event.
        /// </summary>
        void RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Interface for Async Command with parameter
    /// </summary>
    public interface IAsyncCommand<T> : ICommand
    {
        /// <summary>
        /// Execute the command async.
        /// </summary>
        /// <param name="parameter">Parameter to pass to command</param>
        /// <returns>Task to be awaited on.</returns>
        Task ExecuteAsync(T parameter);

        /// <summary>
        /// Raise a CanExecute change event.
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}
