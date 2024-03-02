using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Xml.Serialization;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class SetItem
    {
        [Description("The index to get.")]
        public int Index { get; set; }

        [XmlIgnore]
        [Description("The value to assign to the item.")]
        public Pythonnet.PyObject Value { get; set; }

        public IObservable<Pythonnet.PyObject> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!Pythonnet.PySequence.IsSequenceType(obj))
                    {
                        throw new Exception($"PyObject is not a type of sequence.");
                    }
                    obj.SetItem(Index, Value);
                    return obj;
                }
            });
        }
    }
}