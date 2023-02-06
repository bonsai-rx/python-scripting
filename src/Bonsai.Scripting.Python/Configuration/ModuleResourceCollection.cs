using System.Collections.ObjectModel;

namespace Bonsai.Scripting.Python.Configuration
{
    /// <summary>
    /// Represents a collection of module resource configuration objects.
    /// </summary>
    public class ModuleResourceCollection : KeyedCollection<string, ModuleConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(ModuleConfiguration item)
        {
            return item.Name;
        }
    }
}
