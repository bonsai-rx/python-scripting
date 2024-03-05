using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that evaluates a Python expression in the specified
    /// top-level module.
    /// </summary>
    [DefaultProperty(nameof(Expression))]
    [Description("Evaluates a Python expression in the specified runtime scope.")]
    public class Eval : Combinator<PyObject>
    {
        /// <summary>
        /// Gets or sets the top-level module on which to evaluate the Python expression.
        /// </summary>
        [XmlIgnore]
        [Description("The top-level module on which to evaluate the Python expression.")]
        public PyModule Module { get; set; }

        /// <summary>
        /// Gets or sets the Python expression to evaluate.
        /// </summary>
        [Description("The Python expression to evaluate.")]
        public string Expression { get; set; }

        /// <summary>
        /// Evaluates a Python expression in the specified top-level module whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to trigger evaluation of the Python expression.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyObject"/> handles representing the result
        /// of evaluating the Python expression.
        /// </returns>
        public override IObservable<PyObject> Process<TSource>(IObservable<TSource> source)
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var module = Module ?? runtime.MainModule;
                return source.Select(_ => module.Eval(Expression));
            });
        }

        /// <summary>
        /// Evaluates a Python expression in an observable sequence of modules.
        /// </summary>
        /// <param name="source">
        /// The sequence of modules in which to evaluate the Python expression.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyObject"/> handles representing the result
        /// of evaluating the Python expression.
        /// </returns>
        public IObservable<PyObject> Process(IObservable<PyModule> source)
        {
            return source.Select(module => module.Eval(Expression));
        }

        /// <summary>
        /// Evaluates an expression in the main module of the Python runtime.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the Python runtime in which to evaluate the expression.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyObject"/> handles representing the result
        /// of evaluating the Python expression.
        /// </returns>
        public IObservable<PyObject> Process(IObservable<RuntimeManager> source)
        {
            return source.Select(runtime => runtime.MainModule.Eval(Expression));
        }
    }
}
