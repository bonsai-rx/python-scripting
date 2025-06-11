using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents the creation of a string python data type.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Source)]
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

        public IObservable<Pythonnet.PyString> Process<TSource>(IObservable<TSource> source)
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