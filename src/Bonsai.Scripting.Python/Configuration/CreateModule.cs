using System.ComponentModel;
using System.IO;
using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for top-level modules.
    /// </summary>
    public class CreateModule : ModuleConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the Python script file to run on module initialization.
        /// </summary>
        [FileNameFilter("Python Files (*.py)|*.py|All Files|*.*")]
        [Description("The name of the Python script file to run on module initialization.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        public string Script { get; set; }

        /// <inheritdoc/>
        public override PyObject CreateResource(ResourceManager resourceManager)
        {
            var scope = Py.CreateScope(Name);
            if (!string.IsNullOrEmpty(Script))
            {
                var code = File.ReadAllText(Script);
                scope.Exec(code);
            }
            return scope;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var script = Script;
            if (string.IsNullOrEmpty(script))
            {
                return base.ToString();
            }

            var baseName = Name;
            if (string.IsNullOrEmpty(baseName)) baseName = nameof(CreateModule);
            return $"{baseName} [{script}]";
        }
    }
}
