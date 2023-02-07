using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that executes a Python script in the specified
    /// top-level module.
    /// </summary>
    [DefaultProperty(nameof(Script))]
    [Description("Executes a Python script in the specified top-level module.")]
    public class Exec : Combinator<PyModule>
    {
        /// <summary>
        /// Gets or sets the name of the top-level module on which to execute the Python script.
        /// </summary>
        [TypeConverter(typeof(ModuleNameConverter))]
        [Description("The name of the top-level module on which to execute the Python script.")]
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the Python script to evaluate.
        /// </summary>
        [Description("The Python script to evaluate.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Script { get; set; }

        /// <summary>
        /// Executes a Python script in the specified top-level module whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to trigger execution of the Python script.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyModule"/> objects representing the top-level
        /// module where each Python script was executed.
        /// </returns>
        public override IObservable<PyModule> Process<TSource>(IObservable<TSource> source)
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var module = runtime.Resources.Load<PyModule>(ModuleName);
                return source.Select(_ =>
                {
                    using (Py.GIL())
                    {
                        return module.Exec(Script);
                    }
                });
            });
        }
    }
}
