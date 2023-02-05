using System.Collections.ObjectModel;

namespace Bonsai.Scripting.Python.Configuration
{
    /// <summary>
    /// Represents a collection of scope configuration objects.
    /// </summary>
    public class ScopeConfigurationCollection : KeyedCollection<string, ScopeConfiguration>
    {
        /// <summary>
        /// Returns the key for the specified configuration object.
        /// </summary>
        /// <param name="item">The configuration object from which to extract the key.</param>
        /// <returns>The key for the specified configuration object.</returns>
        protected override string GetKeyForItem(ScopeConfiguration item)
        {
            return item.Name;
        }
    }
}
