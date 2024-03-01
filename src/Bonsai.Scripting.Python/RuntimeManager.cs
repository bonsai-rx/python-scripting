using System;
using System.IO;
using System.Reflection;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Python.Runtime;
using SystemPath = System.IO.Path;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Provides functionality for initializing and managing resources held
    /// by the Python runtime and an interface for scheduling work in the
    /// runtime scheduler.
    /// </summary>
    public class RuntimeManager : IDisposable
    {
        const char AssemblySeparator = ':';
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

        static bool IsEmbeddedResourcePath(string path)
        {
            var separatorIndex = path.IndexOf(AssemblySeparator);
            return separatorIndex >= 0 && !SystemPath.IsPathRooted(path);
        }

        static string ReadAllText(string path)
        {
            if (IsEmbeddedResourcePath(path))
            {
                var nameElements = path.Split(new[] { AssemblySeparator }, 2);
                if (string.IsNullOrEmpty(nameElements[0]))
                {
                    throw new InvalidOperationException(
                        "The embedded resource path \"" + path +
                        "\" must be qualified with a valid assembly name.");
                }

                var assembly = Assembly.Load(nameElements[0]);
                var resourceName = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                using var resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream == null)
                {
                    throw new InvalidOperationException(
                        "The specified embedded resource \"" + nameElements[1] +
                        "\" was not found in assembly \"" + nameElements[0] + "\"");
                }
                
                using var reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
            else return File.ReadAllText(path);
        }

        internal static DynamicModule CreateModule(string name = "", string scriptPath = "")
        {
            using (Py.GIL())
            {
                var module = new DynamicModule(name);
                if (!string.IsNullOrEmpty(scriptPath))
                {
                    try
                    {
                        var code = ReadAllText(scriptPath);
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
                path = EnvironmentHelper.GetEnvironmentPath(path);
                var config = EnvironmentHelper.GetEnvironmentConfig(path);
                Runtime.PythonDLL = EnvironmentHelper.GetPythonDLL(config);
                EnvironmentHelper.SetRuntimePath(config.PythonHome);
                PythonEngine.PythonHome = config.PythonHome;
                if (config.PythonHome != path)
                {
                    PythonEngine.PythonPath = EnvironmentHelper.GetPythonPath(config);
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
