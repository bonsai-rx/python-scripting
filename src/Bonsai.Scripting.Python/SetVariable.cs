using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    public class SetVariable : Sink
    {
        [TypeConverter(typeof(ScopeNameConverter))]
        public string ScopeName { get; set; }

        public string VariableName { get; set; }

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
