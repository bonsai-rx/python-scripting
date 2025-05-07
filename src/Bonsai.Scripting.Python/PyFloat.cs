using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents the creation of a float python data type.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Source)]
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

        public IObservable<Pythonnet.PyFloat> Process<TSource>(IObservable<TSource> source)
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