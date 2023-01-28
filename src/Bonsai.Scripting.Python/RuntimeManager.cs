using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bonsai.Resources;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    public class RuntimeManager : IDisposable
    {
        readonly EventLoopScheduler runtimeScheduler;
        readonly ResourceManager runtimeResources;
        readonly IObserver<RuntimeManager> runtimeObserver;
        IntPtr threadState;

        internal RuntimeManager(string path, IObserver<RuntimeManager> observer)
        {
            runtimeScheduler = new EventLoopScheduler();
            runtimeResources = new ResourceManager();
            runtimeObserver = observer;
            Schedule(() =>
            {
                Initialize(path);
                threadState = PythonEngine.BeginAllowThreads();
                using (Py.GIL())
                {
                    observer.OnNext(this);
                }
            });
        }

        internal static IObservable<RuntimeManager> RuntimeSource { get; } = Observable.Using(
            () => SubjectManager.ReserveSubject(),
            disposable => disposable.Subject);

        internal ResourceManager Resources => runtimeResources;

        internal void Schedule(Action action)
        {
            runtimeScheduler.Schedule(() =>
            {
                try { action(); }
                catch (Exception ex)
                {
                    runtimeObserver.OnError(ex);
                }
            });
        }

        static void Initialize(string path)
        {
            if (!PythonEngine.IsInitialized)
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
                }

                path = Path.GetFullPath(path);
                var pythonHome = EnvironmentHelper.GetPythonHome(path);
                Runtime.PythonDLL = EnvironmentHelper.GetPythonDLL(pythonHome);
                EnvironmentHelper.SetRuntimePath(pythonHome);
                PythonEngine.PythonHome = pythonHome;
                if (pythonHome != path)
                {
                    var version = PythonEngine.Version;
                    PythonEngine.PythonPath = EnvironmentHelper.GetPythonPath(path);
                }
                PythonEngine.Initialize();
            }
        }

        void DisposeInternal()
        {
            using (Py.GIL())
            {
                Resources.Dispose();
            }
            PythonEngine.EndAllowThreads(threadState);
        }

        public void Dispose()
        {
            Schedule(() =>
            {
                DisposeInternal();
                runtimeScheduler.Dispose();
            });
        }
    }
}
