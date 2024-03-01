using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that wraps the source sequence to ensure all notifications
    /// are emitted while holding the Python global interpreter lock.
    /// </summary>
    [Description("Wraps the source sequence to ensure all notifications are emitted while holding the Python global interpreter lock.")]
    public class ObserveOnGIL : Combinator
    {
        /// <summary>
        /// Wraps an observable sequence to ensure all notifications are emitted
        /// while holding the Python global interpreter lock.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to wrap.</param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of ensuring that
        /// all notifications are emitted inside the Python global interpreter lock.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return RuntimeManager.RuntimeSource.SelectMany(_ => source.ObserveOnGIL());
        }
    }
}
