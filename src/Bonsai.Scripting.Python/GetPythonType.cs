using System;
using System.Reactive.Linq;
using Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that gets the type of a python object. Equivalent to calling type(obj) in python.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class GetPythonType
    {

        public IObservable<PyObject> Process(IObservable<PyObject> source)
        {
            return source.Select(obj => 
            {
                using (Py.GIL())
                {
                    return obj.GetPythonType();
                }
            });
        }
    }
}