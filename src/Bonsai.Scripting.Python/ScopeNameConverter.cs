using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    class ScopeNameConverter : ResourceNameConverter
    {
        public ScopeNameConverter()
            : base(typeof(PyModule))
        {
        }

        protected override bool IsResourceSupported(IResourceConfiguration resource)
        {
            return base.IsResourceSupported(resource);
        }
    }
}
