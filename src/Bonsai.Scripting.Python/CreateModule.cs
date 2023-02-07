using System;
using System.ComponentModel;
using System.IO;
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
            return Generate(RuntimeManager.RuntimeSource);
        }

        /// <summary>
        /// Generates an observable sequence that contains the created top-level modules.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of the <see cref="RuntimeManager"/> in which to create
        /// the top-level module.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyModule"/> objects representing all the created
        /// top-level modules.
        /// </returns>
        public IObservable<PyModule> Generate(IObservable<RuntimeManager> source)
        {
            return source.SelectMany(runtime => Observable.Create<PyModule>(observer =>
            {
                using (Py.GIL())
                {
                    var module = new RuntimeModule(runtime, Name);
                    if (!string.IsNullOrEmpty(ScriptPath))
                    {
                        var code = File.ReadAllText(ScriptPath);
                        module.Scope.Exec(code);
                    }
                    observer.OnNext(module.Scope);
                    return module;
                }
            }));
        }
    }
}
