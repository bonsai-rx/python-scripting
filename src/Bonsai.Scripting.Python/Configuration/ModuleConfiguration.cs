using System.Xml.Serialization;
using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python.Configuration
{
    /// <summary>
    /// Provides the abstract base class for configuring and loading module resources.
    /// </summary>
    [XmlInclude(typeof(CreateModule))]
    [XmlInclude(typeof(ImportModule))]
    public abstract class ModuleConfiguration : ResourceConfiguration<PyObject>
    {
    }
}
