using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    public class Eval : Combinator<PyObject>
    {
        [TypeConverter(typeof(ScopeNameConverter))]
        public string ScopeName { get; set; }

        public string Expression { get; set; }

        public override IObservable<PyObject> Process<TSource>(IObservable<TSource> source)
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var scope = runtime.Resources.Load<PyModule>(ScopeName);
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
