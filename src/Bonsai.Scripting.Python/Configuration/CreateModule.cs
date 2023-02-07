using System.ComponentModel;
using System.IO;
using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for top-level modules.
    /// </summary>
    public class CreateModule : ResourceConfiguration<PyModule>
    {
        /// <summary>
        /// Gets or sets the path to the Python script file to run on module initialization.
        /// </summary>
        [FileNameFilter("Python Files (*.py)|*.py|All Files|*.*")]
        [Description("The path to the Python script file to run on module initialization.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        public string ScriptPath { get; set; }

        /// <inheritdoc/>
        public override PyModule CreateResource(ResourceManager resourceManager)
        {
            var scope = Py.CreateScope(Name);
            if (!string.IsNullOrEmpty(ScriptPath))
            {
                var code = File.ReadAllText(ScriptPath);
                scope.Exec(code);
            }
            return scope;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var script = ScriptPath;
            var baseName = Name;
            if (string.IsNullOrEmpty(baseName)) baseName = nameof(CreateModule);
            if (string.IsNullOrEmpty(script)) return baseName;
            else return $"{baseName} [{script}]";
        }
    }
}
