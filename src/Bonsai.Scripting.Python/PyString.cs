using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class PyString
    {

        [Description("The value of the string.")]
        public string Value { get; set; }

        public IObservable<Pythonnet.PyString> Process()
        {
            return Observable.Defer(() =>
            {
                using (Pythonnet.Py.GIL())
                {
                    return Observable.Return(new Pythonnet.PyString(Value));
                }
            });
        }

        public IObservable<Pythonnet.PyString> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!Pythonnet.PyString.IsStringType(obj))
                    {
                        throw new ArgumentException("PyObject must be a type of string.");
                    }
                    return new Pythonnet.PyString(obj);
                }
            });
        }

        public IObservable<Pythonnet.PyString> Process(IObservable<object> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    return new Pythonnet.PyString(Value);
                }
            });
        }
    }
}