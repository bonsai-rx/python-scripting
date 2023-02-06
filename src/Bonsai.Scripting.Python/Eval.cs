using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that evaluates a Python expression in the specified
    /// top-level module.
    /// </summary>
    [Description("Evaluates a Python expression in the specified top-level module.")]
    public class Eval : Combinator<PyObject>
    {
        /// <summary>
        /// Gets or sets the name of the top-level module on which to evaluate the Python expression.
        /// </summary>
        [TypeConverter(typeof(ScopeNameConverter))]
        [Description("The name of the top-level module on which to evaluate the Python expression.")]
        public string ModuleName { get; set; }

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
                var scope = (PyModule)runtime.Resources.Load<PyObject>(ModuleName);
                return source.Select(_ =>
                {
                    using (Py.GIL())
                    {
                        return scope.Eval(Expression);
                    }
                });
            });
        }
    }
}
