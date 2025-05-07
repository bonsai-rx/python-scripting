using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents the creation of an int python data type.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Source)]
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
        
        public IObservable<Pythonnet.PyInt> Process<TSource>(IObservable<TSource> source)
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