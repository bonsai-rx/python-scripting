using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Provides functionality for initializing and managing resources held
    /// by the Python runtime and an interface for scheduling work in the
    /// runtime scheduler.
    /// </summary>
    public class RuntimeManager : IDisposable
    {
        readonly EventLoopScheduler runtimeScheduler;
        readonly IObserver<RuntimeManager> runtimeObserver;
        IntPtr threadState;

        internal RuntimeManager(string pythonHome, string scriptPath, IObserver<RuntimeManager> observer)
        {
            runtimeScheduler = new EventLoopScheduler();
            runtimeObserver = observer;
            Schedule(() =>
            {
                Initialize(pythonHome);
                threadState = PythonEngine.BeginAllowThreads();
                using (Py.GIL())
                {
                    MainModule = CreateModule(scriptPath: scriptPath);
                }
                observer.OnNext(this);
            });
        }

        internal PyModule MainModule { get; private set; }

        internal static IObservable<RuntimeManager> RuntimeSource { get; } = Observable.Using(
            () => SubjectManager.ReserveSubject(),
            disposable => disposable.Subject)
            .Take(1);

        internal static DynamicModule CreateModule(string name = "", string scriptPath = "")
        {
            using (Py.GIL())
            {
                var module = new DynamicModule(name);
                if (!string.IsNullOrEmpty(scriptPath))
                {
                    try
                    {
                        var code = File.ReadAllText(scriptPath);
                        module.Exec(code);
                    }
                    catch (Exception)
                    {
                        module.Dispose();
                        throw;
                    }
                }
                return module;
            }
        }

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
                path = EnvironmentHelper.GetVirtualEnvironmentPath(path);
                EnvironmentHelper.SetVirtualEnvironmentPath(path);

                var pythonHome = EnvironmentHelper.GetPythonHome(path);
                var pythonVersion = EnvironmentHelper.GetPythonVersion(path, pythonHome);
                var pythonDLL = EnvironmentHelper.GetPythonDLL(pythonHome, pythonVersion);
                
                Runtime.PythonDLL = pythonDLL;

                // Only set environment/python.net variables if a virtual environment is used, otherwise use default python configuration
                if (!string.IsNullOrEmpty(path))
                {

                    EnvironmentHelper.SetRuntimePath(pythonHome, path, pythonVersion);
                    PythonEngine.PythonHome = pythonHome;

                    if (pythonHome != path)
                    {
                        var basePath = PythonEngine.PythonPath;
                        var pythonPath = EnvironmentHelper.GetPythonPath(pythonHome, path, basePath, pythonDLL);
                        PythonEngine.PythonPath = pythonPath;
                    }
                }

                PythonEngine.Initialize();
            }
        }

        /// <summary>
        /// Shutdown the thread and release all resources associated with the Python runtime.
        /// All remaining work scheduled after shutdown is abandoned.
        /// </summary>
        public void Dispose()
        {
            Schedule(() =>
            {
                if (PythonEngine.IsInitialized)
                {
                    if (MainModule != null)
                    {
                        using (Py.GIL())
                        {
                            MainModule.Dispose();
                        }
                    }
                    PythonEngine.EndAllowThreads(threadState);
                    PythonEngine.Shutdown();
                }
                runtimeScheduler.Dispose();
            });
        }
    }
}
