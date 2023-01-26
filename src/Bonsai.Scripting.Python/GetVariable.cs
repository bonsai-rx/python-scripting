using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    public class GetVariable : Source<PyObject>
    {
        [TypeConverter(typeof(ScopeNameConverter))]
        public string ScopeName { get; set; }

        public string VariableName { get; set; }

        public override IObservable<PyObject> Generate()
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var scope = runtime.Resources.Load<PyModule>(ScopeName);
                return Observable.Return(scope.Get(VariableName));
            });
        }

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
