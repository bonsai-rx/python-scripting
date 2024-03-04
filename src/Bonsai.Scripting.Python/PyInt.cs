using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class PyInt
    {

        [Description("The value of the integer.")]
        public int Value { get; set; }

        public IObservable<Pythonnet.PyInt> Process()
        {
            return Observable.Defer(() =>
            {
                using (Pythonnet.Py.GIL())
                {
                    return Observable.Return(new Pythonnet.PyInt(Value));
                }
            });
        }

        public IObservable<Pythonnet.PyInt> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!Pythonnet.PyNumber.IsNumberType(obj))
                    {
                        throw new ArgumentException("PyObject must be a type of number.");
                    }
                    return new Pythonnet.PyInt(obj);
                }
            });
        }

        public IObservable<Pythonnet.PyInt> Process(IObservable<object> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    return new Pythonnet.PyInt(Value);
                }
            });
        }
    }
}