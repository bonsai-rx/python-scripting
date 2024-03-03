using System;
using System.Collections;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class PyTuple
    {

        // public IObservable<Pythonnet.PyTuple> Process(IObservable<Pythonnet.PyObject> source)
        // {
        //     return source.Select(obj =>
        //     {
        //         using (Pythonnet.Py.GIL())
        //         {
        //             if (!Pythonnet.PySequence.IsSequenceType(obj))
        //             {
        //                 throw new ArgumentException("PyObject must be a type of tuple.");
        //             }
        //             return Pythonnet.PyTuple.AsTuple(obj);
        //         }
        //     });
        // }

        public IObservable<Pythonnet.PyTuple> Process(IObservable<object> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!(obj is ITuple || obj is IList || obj is Array))
                    {
                        throw new ArgumentException("Input must be a type of tuple, list, or array.");
                    }

                    PropertyInfo[] properties = obj.GetType().GetProperties();
                    Pythonnet.PyObject[] pyObjects = new Pythonnet.PyObject[properties.Length];

                    for (int i = 0; i < properties.Length; i++)
                    {
                        object value = properties[i].GetValue(obj, null);

                        if (!(value is Pythonnet.PyObject))
                        {
                            throw new ArgumentException($"All elements of the tuple must be of type PyObject. Instead, found {value.GetType()} for Item{i+1}.");
                        }

                        pyObjects[i] = (Pythonnet.PyObject)value;
                    }
                    return new Pythonnet.PyTuple(pyObjects);
                }
            });
        }
    }
}