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
    /// Represents an operator that will invoke a callable object and pass the given arguments to the call. The input can either be the callable object itself, or can be an object which has a named callable attribute.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class Invoke
    {
        [Description("The name of the callable object to invoke.")]
        public string Callable { get; set; }

        [XmlIgnore]
        [Description("The args to pass to the callable.")]
        public Pythonnet.PyTuple Args { get; set; } = null;

        public IObservable<Pythonnet.PyObject> Process(IObservable<Pythonnet.PyObject> source)
        {
            return source.Select(obj =>
            {
                using (Pythonnet.Py.GIL())
                {
                    var callable = string.IsNullOrEmpty(Callable) ? obj : obj.GetAttr(Callable);
                    if (!callable.IsCallable())
                    {
                        throw new Exception($"Cannot invoke callable: {callable} because it is not callable.");
                    }
                    var args = Args == null ? new Pythonnet.PyTuple() : Args;
                    return callable.Invoke(args);
                }
            });
        }
    }
}