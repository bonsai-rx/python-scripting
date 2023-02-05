using System.ComponentModel;
using System.IO;
using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python.Configuration
{
    public class ScopeConfiguration : ResourceConfiguration<PyModule>
    {
        [FileNameFilter("Python Files (*.py)|*.py|All Files|*.*")]
        [Description("The name of the Python script file to run on scope initialization.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        public string Script { get; set; }

        public override PyModule CreateResource(ResourceManager resourceManager)
        {
            var scope = Py.CreateScope(Name);
            if (!string.IsNullOrEmpty(Script))
            {
                var code = File.ReadAllText(Script);
                scope.Exec(code);
            }
            return scope;
        }

        public override string ToString()
        {
            var script = Script;
            var baseName = Name;
            if (string.IsNullOrEmpty(baseName)) baseName = nameof(ScopeConfiguration);
            if (string.IsNullOrEmpty(script)) return baseName;
            else return $"{baseName} [{script}]";
        }
    }
}
