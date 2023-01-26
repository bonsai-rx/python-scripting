using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.Resources;
using Bonsai.Scripting.Python.Configuration;

namespace Bonsai.Scripting.Python
{
    [DefaultProperty(nameof(Scopes))]
    public class ScopeResources : ResourceLoader
    {
        [Editor("Bonsai.Resources.Design.ResourceCollectionEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        public ScopeConfigurationCollection Scopes { get; } = new ScopeConfigurationCollection();

        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return Scopes;
        }

        public IObservable<ResourceConfigurationCollection> Process(IObservable<RuntimeManager> source)
        {
            return source.Select(runtime =>
            {
                return new ResourceConfigurationCollection(runtime.Resources, GetResources());
            });
        }
    }
}
