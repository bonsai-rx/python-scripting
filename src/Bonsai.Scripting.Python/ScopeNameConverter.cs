using Bonsai.Resources;
using Bonsai.Scripting.Python.Configuration;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    class ScopeNameConverter : ResourceNameConverter
    {
        public ScopeNameConverter()
            : base(typeof(PyObject))
        {
        }

        protected override bool IsResourceSupported(IResourceConfiguration resource)
        {
            return resource is CreateModule;
        }
    }
}
