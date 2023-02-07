using System;
using System.Threading;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    class DynamicModule : PyModule
    {
        PythonEngine.ShutdownHandler shutdown;

        internal DynamicModule(string name)
            : base(name ?? throw new ArgumentNullException(name))
        {
            shutdown = () =>
            {
                if (Interlocked.Exchange(ref shutdown, null) != null)
                {
                    Dispose();
                }
            };
            PythonEngine.AddShutdownHandler(shutdown);
        }

        protected override void Dispose(bool disposing)
        {
            var handler = Interlocked.Exchange(ref shutdown, null);
            if (handler != null)
            {
                PythonEngine.RemoveShutdownHandler(handler);
                using (Py.GIL())
                {
                    base.Dispose(disposing);
                }
            }
            else base.Dispose(disposing);
        }
    }
}
