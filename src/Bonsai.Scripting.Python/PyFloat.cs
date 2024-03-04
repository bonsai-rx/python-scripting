using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class PyFloat
    {

        [Description("The value of the float.")]
        public double Value { get; set; }

        public IObservable<Pythonnet.PyFloat> Process()
        {
            return Observable.Defer(() =>
            {
                using (Pythonnet.Py.GIL())
                {
                    return Observable.Return(new Pythonnet.PyFloat(Value));
                }
            });
        }

        public IObservable<Pythonnet.PyFloat> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!Pythonnet.PyNumber.IsNumberType(obj))
                    {
                        throw new ArgumentException("PyObject must be a type of number.");
                    }
                    return new Pythonnet.PyFloat(obj);
                }
            });
        }

        public IObservable<Pythonnet.PyFloat> Process(IObservable<object> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    return new Pythonnet.PyFloat(Value);
                }
            });
        }
    }
}