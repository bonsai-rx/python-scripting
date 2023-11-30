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
            Console.WriteLine($"Python engine initialized? {PythonEngine.IsInitialized}.");
            if (!PythonEngine.IsInitialized)
            {
                Console.WriteLine($"Starting path: {path}");
                if (string.IsNullOrEmpty(path))
                {
                    path = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
                    Console.WriteLine($"Virtual env path: {path}");
                }

                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFullPath(path);
                    Console.WriteLine($"Base Path: {path}");
                }

                var pythonHome = EnvironmentHelper.GetPythonHome(path);
                Console.WriteLine($"Python Home: {pythonHome}");

                var pythonVersion = EnvironmentHelper.GetPythonVersion(path, pythonHome);
                Console.WriteLine($"Python Version: {pythonVersion}");

                var pythonDLL = EnvironmentHelper.GetPythonDLL(pythonVersion);
                Console.WriteLine($"Python DLL: {pythonDLL}");
                Runtime.PythonDLL = pythonDLL;

                var systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).TrimEnd(Path.PathSeparator);
                if (!systemPath.Split(Path.PathSeparator).Contains(pythonHome, StringComparer.OrdinalIgnoreCase))
                {
                    systemPath = string.IsNullOrEmpty(systemPath) ? pythonHome : pythonHome + Path.PathSeparator + path;
                }
                Console.WriteLine($"System Path: {systemPath}");
                Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);

                PythonEngine.PythonHome = pythonHome;

                // Console.WriteLine($"Python Engine Path: {PythonEngine.PythonPath}");

                if (pythonHome != path)
                {
                    var pythonPath = EnvironmentHelper.GetPythonPath(pythonHome, path, pythonVersion);
                    Console.WriteLine($"Python Path: {pythonPath}");
                    PythonEngine.PythonPath = pythonPath;
                }

                Console.WriteLine($"Python engine python path: {PythonEngine.PythonPath}");
                Console.WriteLine($"Python engine build info: {PythonEngine.BuildInfo}");

                Console.WriteLine("Initializing python engine...");
                PythonEngine.Initialize();

                Console.WriteLine($"Python engine python home: {PythonEngine.PythonHome}");
                Console.WriteLine($"Python engine version: {PythonEngine.Version}");
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
                    Console.WriteLine("Shutting down...");
                    PythonEngine.EndAllowThreads(threadState);
                    PythonEngine.Shutdown();
                }
                runtimeScheduler.Dispose();
            });
        }
    }
}
