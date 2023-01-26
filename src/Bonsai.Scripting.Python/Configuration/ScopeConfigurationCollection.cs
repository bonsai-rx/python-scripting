using System.Collections.ObjectModel;

namespace Bonsai.Scripting.Python.Configuration
{
    public class ScopeConfigurationCollection : KeyedCollection<string, ScopeConfiguration>
    {
        protected override string GetKeyForItem(ScopeConfiguration item)
        {
            return item.Name;
        }
    }
}
