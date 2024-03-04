using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that creates a top-level module in the Python runtime.
    /// </summary>
    public class CreateModule : Source<PyModule>
    {
        /// <summary>
        /// Gets or sets the name of the top-level module.
        /// </summary>
        [Description("The name of the top-level module.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path to the Python script file to run on module initialization.
        /// </summary>
        [FileNameFilter("Python Files (*.py)|*.py|All Files|*.*")]
        [Description("The path to the Python script file to run on module initialization.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ScriptPath { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains the created top-level module.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="PyModule"/> class
        /// representing the created top-level module.
        /// </returns>
        public override IObservable<PyModule> Generate()
        {
            var name = Name;
            var scriptPath = ScriptPath;
            return RuntimeManager.RuntimeSource
                .ObserveOnGIL()
                .Select(_ => RuntimeManager.CreateModule(name ?? string.Empty, scriptPath));
        }
    }
}
