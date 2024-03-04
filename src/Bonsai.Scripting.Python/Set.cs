using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that adds or updates a Python runtime variable in the
    /// specified top-level module.
    /// </summary>
    [DefaultProperty(nameof(VariableName))]
    [Description("Adds or updates a Python runtime variable in the specified top-level module.")]
    public class Set : Sink
    {
        /// <summary>
        /// Gets or sets the Python top-level module containing the variable.
        /// </summary>
        [XmlIgnore]
        [Description("The Python top-level module containing the variable.")]
        public PyModule Module { get; set; }

        /// <summary>
        /// Gets or sets the name of the variable to add or update the value of.
        /// </summary>
        [Description("The name of the variable to add or update the value of.")]
        public string VariableName { get; set; }

        /// <summary>
        /// Adds or updates a Python runtime variable in the specified top-level module
        /// with the values from an observable sequence.
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
            return RuntimeManager.RuntimeSource
                .GetModuleOrDefaultAsync(Module)
                .SelectMany(module => source.Do(value => module.Set(VariableName, value)));
        }
    }
}
