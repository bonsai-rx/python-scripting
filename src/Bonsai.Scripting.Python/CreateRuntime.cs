using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that creates a Python runtime object which can be used to
    /// import modules, evaluate expressions, and pass data to and from a Python scope.
    /// </summary>
    [DefaultProperty(nameof(PythonHome))]
    [Description("Creates a Python runtime object which can be used to import modules, evaluate expressions, and pass data to and from a Python scope.")]
    public class CreateRuntime : Source<RuntimeManager>
    {
        /// <summary>
        /// Gets or sets the location where the standard Python libraries are installed.
        /// </summary>
        /// <remarks>
        /// If no location is specified, the runtime will be created from the currently
        /// activated Python virtual environment.
        /// </remarks>
        [Description("The location where the standard Python libraries are installed.")]
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string PythonHome { get; set; }

        /// <summary>
        /// Creates an observable sequence that initializes a Python runtime object which
        /// can be used to import modules, evaluate expressions, and pass data to and
        /// from a Python scope.
        /// </summary>
        /// <returns>
        /// An observable sequence that initializes and returns a <see cref="RuntimeManager"/>
        /// object on subscription. On cancellation, the runtime object is disposed.
        /// </returns>
        public override IObservable<RuntimeManager> Generate()
        {
            return Observable.Create<RuntimeManager>(observer =>
            {
                var disposable = SubjectManager.ReserveSubject();
                var subscription = disposable.Subject.SubscribeSafe(observer);
                var runtime = new RuntimeManager(PythonHome, disposable.Subject);
                return new CompositeDisposable
                {
                    subscription,
                    disposable,
                    runtime
                };
            });
        }
    }
}
