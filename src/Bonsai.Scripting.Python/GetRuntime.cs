using System;
using System.ComponentModel;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that gets the Python runtime object which can be used
    /// to import modules, evaluate expressions, and pass data to and from Python.
    /// </summary>
    /// <remarks>
    /// The runtime object notification is emitted while holding the global interpreter lock.
    /// </remarks>
    [Description("Gets the Python runtime object which can be used to import modules, evaluate expressions, and pass data to and from Python.")]
    public class GetRuntime : Source<RuntimeManager>
    {
        /// <summary>
        /// Wraps an observable sequence to ensure all notifications are emitted
        /// while holding the Python global interpreter lock.
        /// </summary>
        /// <returns>
        /// An observable sequence that returns the active <see cref="RuntimeManager"/>
        /// object on subscription. The value is emitted while holding the global
        /// interpreter lock.
        /// </returns>
        public override IObservable<RuntimeManager> Generate()
        {
            return RuntimeManager.RuntimeSource.ObserveOnGIL();
        }
    }
}
