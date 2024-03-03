using System;
using System.Collections;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class PyList
    {

        public IObservable<Pythonnet.PyList> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!Pythonnet.PySequence.IsSequenceType(obj))
                    {
                        throw new ArgumentException("PyObject must be a type of list.");
                    }
                    return Pythonnet.PyList.AsList(obj);
                }
            });
        }

        public IObservable<Pythonnet.PyList> Process(IObservable<object> source)
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
                            throw new ArgumentException($"All elements of the list must be of type PyObject. Instead, found {value.GetType()} for Item{i}.");
                        }

                        pyObjects[i] = (Pythonnet.PyObject)value;
                    }
                    return new Pythonnet.PyList(pyObjects);
                }
            });
        }
    }
}