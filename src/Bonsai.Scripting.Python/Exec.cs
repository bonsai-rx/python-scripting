using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
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
        /// Gets or sets the top-level module on which to execute the Python script.
        /// </summary>
        [XmlIgnore]
        [Description("The top-level module on which to execute the Python script.")]
        public PyModule Module { get; set; }

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
                return source.Select(_ =>
                {
                    using (Py.GIL())
                    {
                        var module = Module ?? runtime.MainModule;
                        return module.Exec(Script);
                    }
                });
            });
        }

        /// <summary>
        /// Executes a Python script in an observable sequence of modules.
        /// </summary>
        /// <param name="source">
        /// The sequence of modules in which to execute the Python script.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of executing the
        /// Python script in each of the <see cref="PyModule"/> objects.
        /// </returns>
        public IObservable<PyModule> Process(IObservable<PyModule> source)
        {
            return source.Select(module =>
            {
                using (Py.GIL())
                {
                    return module.Exec(Script);
                }
            });
        }

        /// <summary>
        /// Executes a script in the main module of the Python runtime.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the Python runtime in which to execute the script.
        /// </param>
        /// <returns>
        /// A sequence containing the <see cref="PyModule"/> object representing
        /// the top-level module where the Python script was executed.
        /// </returns>
        public IObservable<PyModule> Process(IObservable<RuntimeManager> source)
        {
            return source.Select(runtime =>
            {
                using (Py.GIL())
                {
                    return runtime.MainModule.Exec(Script);
                }
            });
        }
    }
}
