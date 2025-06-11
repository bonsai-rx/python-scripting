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
    /// Represents an operator that gets an attribute from a python object if the attribute exists.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class GetAttr
    {
        [Description("The name of the attribute to get.")]
        public string Attribute { get; set; }

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
                    if (!obj.HasAttr(Attribute))
                    {
                        throw new Exception($"PyObject does not have attribute {Attribute}.");
                    }
                    return obj.GetAttr(Attribute);
                }
            });
        }
    }
}
