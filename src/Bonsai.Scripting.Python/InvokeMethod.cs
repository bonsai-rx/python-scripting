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
    public class InvokeMethod
    {
        [Description("The name of the method to invoke.")]
        public string Method { get; set; }

        [XmlIgnore]
        [Description("The args to pass to the callable.")]
        public Pythonnet.PyTuple Args { get; set; } = null;

        public IObservable<Pythonnet.PyObject> Process(IObservable<Pythonnet.PyObject> source)
        {
            if (string.IsNullOrEmpty(Method))
            {
                throw new Exception("Method name cannot be null or empty.");
            }

            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    var args = Args == null ? new Pythonnet.PyTuple() : Args;
                    return obj.InvokeMethod(Method, args);
                }
            });
        }

    }
}
