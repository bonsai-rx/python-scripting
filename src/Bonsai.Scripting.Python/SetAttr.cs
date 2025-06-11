using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using Python.Runtime;
using System.Xml.Serialization;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that sets the named attribute of an object to a value.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class SetAttr
    {
        [Description("The name of the attribute to get.")]
        public string Attribute { get; set; }

        [XmlIgnore]
        [Description("The object to assign to the attribute.")]
        public PyObject Value { get; set; } = null;

        public IObservable<PyObject> Process(IObservable<PyObject> source)
        {
            if (string.IsNullOrEmpty(Attribute))
            {
                throw new Exception("Attribute cannot be null or empty.");
            }
            return source.Do(obj =>
            {
                using (Py.GIL())
                {
                    if (Value == null)
                    {
                        throw new Exception("Value cannot be null.");
                    }
                    obj.SetAttr(Attribute, Value);
                }
            });
        }
    }
}
