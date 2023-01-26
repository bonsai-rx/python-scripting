using System;
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

        internal RuntimeManager(string pythonPath, IObserver<RuntimeManager> observer)
        {
            runtimeScheduler = new EventLoopScheduler();
            runtimeResources = new ResourceManager();
            runtimeObserver = observer;
            Schedule(() =>
            {
                Initialize(pythonPath);
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

        static void Initialize(string pythonPath)
        {
            if (!PythonEngine.IsInitialized)
            {
                var path = Environment.GetEnvironmentVariable("PATH").TrimEnd(';');
                path = string.IsNullOrEmpty(path) ? pythonPath : path + ";" + pythonPath;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
                Runtime.PythonDLL = "python39.dll";
                PythonEngine.PythonHome = pythonPath;
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
