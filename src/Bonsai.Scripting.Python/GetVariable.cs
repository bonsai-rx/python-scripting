using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that gets the value of a Python runtime variable in the
    /// specified scope.
    /// </summary>
    [Description("Gets the value of a Python runtime variable in the specified scope.")]
    public class GetVariable : Source<PyObject>
    {
        /// <summary>
        /// Gets or sets the name of the Python runtime scope containing the variable.
        /// </summary>
        [TypeConverter(typeof(ScopeNameConverter))]
        [Description("The name of the Python runtime scope containing the variable.")]
        public string ScopeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the scope variable to get the value of.
        /// </summary>
        [Description("The name of the scope variable to get the value of.")]
        public string VariableName { get; set; }

        /// <summary>
        /// Gets the value of a Python runtime variable in the specified scope and
        /// surfaces it through an observable sequence.
        /// </summary>
        /// <returns>
        /// A sequence containing the value of the Python runtime variable as
        /// a <see cref="PyObject"/>.
        /// </returns>
        public override IObservable<PyObject> Generate()
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var scope = runtime.Resources.Load<PyModule>(ScopeName);
                return Observable.Return(scope.Get(VariableName));
            });
        }

        /// <summary>
        /// Gets the value of a Python runtime variable in the specified scope
        /// whenever an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to get the value of the variable.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyObject"/> handles representing the value
        /// of the Python runtime variable.
        /// </returns>
        public IObservable<PyObject> Generate<TSource>(IObservable<TSource> source)
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var scope = runtime.Resources.Load<PyModule>(ScopeName);
                return source.Select(_ =>
                {
                    using (Py.GIL())
                    {
                        return scope.Get(VariableName);
                    }
                });
            });
        }
    }
}
