using System;
using System.IO;
using System.Linq;
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
                if (string.IsNullOrEmpty(path))
                {
                    path = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
                }

                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFullPath(path);
                }

                var pythonHome = EnvironmentHelper.GetPythonHome(path);
                var pythonVersion = EnvironmentHelper.GetPythonVersion(path, pythonHome);
                var pythonDLL = EnvironmentHelper.GetPythonDLL(pythonVersion);
                Runtime.PythonDLL = pythonDLL;
                // EnvironmentHelper.SetEnvironmentPath(pythonHome);
                var systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).TrimEnd(Path.PathSeparator);
                if (!systemPath.Split(Path.PathSeparator).Contains(pythonHome, StringComparer.OrdinalIgnoreCase))
                {
                    systemPath = string.IsNullOrEmpty(systemPath) ? pythonHome : pythonHome + Path.PathSeparator + systemPath;
                }
                Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);
                PythonEngine.PythonHome = pythonHome;
                if (pythonHome != path)
                {
                    var pythonPath = EnvironmentHelper.GetPythonPath(pythonHome, path, pythonVersion);
                    PythonEngine.PythonPath = pythonPath;
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
