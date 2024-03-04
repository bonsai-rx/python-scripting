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
    public class Import
    {
        [Description("The name of the package.")]
        public string Package { get; set; }

        [Description("The \"as name\" of the package. For example, in \"import numpy as np\", np is the as name.")]
        public string AsName { get; set; }

        public IObservable<PyObject> Process(IObservable<PyModule> source)
        {
            if (string.IsNullOrEmpty(Package))
            {
                throw new ArgumentException(nameof(Package), "A package must be specified.");
            }
            return source.Select(module => 
            {
                using (Py.GIL())
                {
                    if (!string.IsNullOrEmpty(AsName))
                    {
                        return module.Import(Package, AsName);
                    }

                    if (!Package.Contains('.'))
                    {
                        return module.Import(Package);
                    }

                    var packages = Package.Split('.');
                    var packagePath = string.Empty;

                    PyObject obj = null;
                    for (int i = 0; i < packages.Length; i++)
                    {
                        packagePath = string.IsNullOrEmpty(packagePath) ? packages[i] : $"{packagePath}.{packages[i]}";
                        obj = module.Import(packagePath);
                    }

                    return obj;
                }
            });
        }
    }
}