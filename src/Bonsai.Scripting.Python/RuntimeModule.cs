using System;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    class RuntimeModule : IDisposable
    {
        public RuntimeModule(RuntimeManager owner, string name)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Name = name ?? string.Empty;
            Scope = Py.CreateScope(Name);
            owner.Modules.Add(Name, this);
            ShutdownHandler = Scope.Dispose;
            PythonEngine.AddShutdownHandler(ShutdownHandler);
        }

        public string Name { get; }

        public PyModule Scope { get; }

        RuntimeManager Owner { get; }

        PythonEngine.ShutdownHandler ShutdownHandler { get; }

        public void Dispose()
        {
            using (Py.GIL())
            {
                PythonEngine.RemoveShutdownHandler(ShutdownHandler);
                Owner.Modules.Remove(Name);
                Scope.Dispose();
            }
        }
    }
}
