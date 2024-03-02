using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using Python.Runtime;
using System.Xml.Serialization;
using System.Linq;

namespace Bonsai.Scripting.Python
{
    [Combinator]
    public class SetAttr
    {
        [Description("The name of the attribute to get.")]
        public string Attribute { get; set; }

        [XmlIgnore]
        [Description("The object to assign to the attribute.")]
        public PyObject Value { get; set; }

        public IObservable<PyObject> Process(IObservable<PyObject> source)
        {
            if (string.IsNullOrEmpty(Attribute))
            {
                throw new Exception("Attribute cannot be null or empty.");
            }
            return source.Select(obj =>
            {
                using (Py.GIL())
                {
                    obj.SetAttr(Attribute, Value);
                    return obj;
                }
            });
        }
    }
}
