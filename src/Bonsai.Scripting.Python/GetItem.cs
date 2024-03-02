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
    public class GetItem
    {
        [Description("The index to get.")]
        public int? Index { get; set; } = null;

        [Description("The key to get.")]
        public string Key { get; set; } = null;

        public IObservable<PyObject> Process(IObservable<PyObject> source)
        {
            if (!(Index.HasValue ^ !string.IsNullOrEmpty(Key)))
            {
                throw new Exception("Either an index or a key must be provided.");
            }

            return source.Select(obj =>
            {
                using (Py.GIL())
                {
                    if (!obj.IsIterable())
                    {
                        throw new Exception($"PyObject is not iterable.");
                    }
                    return Index != null ? obj.GetItem(Index.Value) : obj.GetItem(Key);
                }
            });
        }
    }
}