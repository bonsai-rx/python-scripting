using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.Resources;
using Bonsai.Scripting.Python.Configuration;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that creates a collection of scope resources to
    /// be loaded into the runtime resource manager.
    /// </summary>
    [DefaultProperty(nameof(Scopes))]
    [Description("Creates a collection of scope resources to be loaded into the runtime resource manager.")]
    public class ScopeResources : ResourceLoader
    {
        /// <summary>
        /// Gets the collection of scope resources to be loaded into the runtime resource manager.
        /// </summary>
        [Editor("Bonsai.Resources.Design.ResourceCollectionEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of scope resources to be loaded into the runtime resource manager.")]
        public ScopeConfigurationCollection Scopes { get; } = new ScopeConfigurationCollection();

        /// <inheritdoc/>
        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return Scopes;
        }

        /// <summary>
        /// Bundles a set of resources to be loaded into the runtime resource manager.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the <see cref="RuntimeManager"/> object into which
        /// the resources will be loaded.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ResourceConfigurationCollection"/> objects which
        /// can be used to load additional resources into the resource manager.
        /// </returns>
        public IObservable<ResourceConfigurationCollection> Process(IObservable<RuntimeManager> source)
        {
            return source.Select(runtime =>
            {
                return new ResourceConfigurationCollection(runtime.Resources, GetResources());
            });
        }
    }
}
