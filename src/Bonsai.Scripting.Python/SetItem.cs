using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using Pythonnet = Python.Runtime;
using System.Xml.Serialization;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that sets the named attribute of an object to a value.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Combinator]
    public class SetItem
    {
        [Description("The index to set.")]
        public int Index { get; set; } = 0;

        [XmlIgnore]
        [Description("The value to assign to the index.")]
        public Pythonnet.PyObject Value { get; set; } = null;

        public IObservable<Pythonnet.PyObject> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Do(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    if (!Pythonnet.PySequence.IsSequenceType(obj))
                    {
                        throw new Exception($"PyObject is not a type of sequence.");
                    }
                    if (Value == null)
                    {
                        throw new Exception("Value cannot be null.");
                    }
                    obj.SetItem(Index, Value);
                }
            });
        }
    }
}