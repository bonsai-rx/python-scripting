using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python.Configuration
{
    /// <summary>
    /// Provides functionality for importing named Python modules.
    /// </summary>
    public class ImportModule : ModuleConfiguration
    {
        /// <inheritdoc/>
        public override PyObject CreateResource(ResourceManager resourceManager)
        {
            return Py.Import(Name);
        }
    }
}
