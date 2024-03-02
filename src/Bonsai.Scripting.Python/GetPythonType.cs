using System;
using System.Reactive.Linq;
using Python.Runtime;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    [Combinator]
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