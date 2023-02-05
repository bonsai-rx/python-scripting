using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that adds or updates a Python runtime variable in the
    /// specified scope.
    /// </summary>
    [Description("Adds or updates a Python runtime variable in the specified scope.")]
    public class SetVariable : Sink
    {
        /// <summary>
        /// Gets or sets the name of the Python runtime scope containing the variable.
        /// </summary>
        [TypeConverter(typeof(ScopeNameConverter))]
        [Description("The name of the Python runtime scope containing the variable.")]
        public string ScopeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the scope variable to add or update the value of.
        /// </summary>
        [Description("The name of the scope variable to add or update the value of.")]
        public string VariableName { get; set; }

        /// <summary>
        /// Adds or updates a Python runtime variable in the specified scope with
        /// the values from an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the values in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of values used to update the Python runtime variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of setting the
        /// specified Python runtime variable to the values of the sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var scope = runtime.Resources.Load<PyModule>(ScopeName);
                return source.Do(value =>
                {
                    using (Py.GIL())
                    {
                        scope.Set(VariableName, value);
                    }
                });
            });
        }
    }
}
