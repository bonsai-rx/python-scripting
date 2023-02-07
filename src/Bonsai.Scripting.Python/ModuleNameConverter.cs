using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    class ModuleNameConverter : ResourceNameConverter
    {
        public ModuleNameConverter()
            : base(typeof(PyModule))
        {
        }
    }
}
