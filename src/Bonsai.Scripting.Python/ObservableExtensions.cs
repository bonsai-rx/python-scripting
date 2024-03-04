using System;
using System.Reactive;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    static class ObservableExtensions
    {
        public static IObservable<PyModule> GetModuleOrDefaultAsync(this IObservable<RuntimeManager> source, PyModule module)
        {
            return module != null
                ? Observable.Return(module)
                : source.Select(runtime => runtime.MainModule);
        }

        public static IObservable<TSource> ObserveOnGIL<TSource>(this IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                var sourceObserver = Observer.Create<TSource>(
                    value =>
                    {
                        using (Py.GIL())
                        {
                            observer.OnNext(value);
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(sourceObserver);
            });
        }
    }
}
