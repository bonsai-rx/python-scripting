using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Linq;
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

        internal static bool IsEmbeddedResourcePath(string path)
        {
            var separatorIndex = path.IndexOf(AssemblySeparator);
            return separatorIndex >= 0 && !SystemPath.IsPathRooted(path);
        }

        internal static string GetEmbeddedPythonCode(string path)
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
            var code = reader.ReadToEnd();
            return code;
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
                        var code = IsEmbeddedResourcePath(scriptPath) ? GetEmbeddedPythonCode(scriptPath) : File.ReadAllText(scriptPath);
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
                string venvPath = null;
                if (string.IsNullOrEmpty(path))
                {
                    venvPath = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
                    path = venvPath;
                }

                // Console.WriteLine($"Path: {path}");

                var pythonHome = EnvironmentHelper.GetPythonHome(path);
                // Console.WriteLine($"Python home: {pythonHome}");

                var pythonVersion = EnvironmentHelper.GetPythonVersion(path, pythonHome);
                // Console.WriteLine($"Python version: {pythonVersion}");

                var pythonDLL = EnvironmentHelper.GetPythonDLL(pythonHome, pythonVersion);
                // Console.WriteLine($"Python dll: {pythonDLL}");

                // Set the python DLL
                Runtime.PythonDLL = pythonDLL;
                
                // Only set environment/python.net variables if a virtual environment is used, otherwise use default python configuration
                if (!string.IsNullOrEmpty(path))
                {
                    var pathToVirtualEnv = Path.GetFullPath(path);
                    // Console.WriteLine($"Venv path: {pathToVirtualEnv}");

                    if (string.IsNullOrEmpty(venvPath))
                    {
                        Environment.SetEnvironmentVariable("VIRTUAL_ENV", pathToVirtualEnv);
                        var virtualEnvPrompt = $"({Path.GetFileName(pathToVirtualEnv)})";
                        Environment.SetEnvironmentVariable("VIRTUAL_ENV_PROMPT", virtualEnvPrompt);
                        var ps1 = Environment.GetEnvironmentVariable("PS1");
                        ps1 = string.IsNullOrEmpty(ps1) ? virtualEnvPrompt : virtualEnvPrompt + " " + ps1;
                        Environment.SetEnvironmentVariable("PS1", ps1);
                    }

                    string systemPath = Environment.GetEnvironmentVariable("PATH")!.TrimEnd(Path.PathSeparator);
                    systemPath = string.IsNullOrEmpty(systemPath) ? $"{pathToVirtualEnv}/bin" : $"{pathToVirtualEnv}/bin" + Path.PathSeparator + systemPath;
                    Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                    var pythonBase = Path.GetDirectoryName(pythonHome);
                    Environment.SetEnvironmentVariable("PYTHONPATH", string.Join(Path.PathSeparator.ToString(),
                        pathToVirtualEnv,
                        $"{pythonBase}/lib/python{pythonVersion}.zip",
                        $"{pythonBase}/lib/python{pythonVersion}",
                        $"{pythonBase}/lib/python{pythonVersion}/lib-dynload",
                        $"{pathToVirtualEnv}/lib/python{pythonVersion}/site-packages"), EnvironmentVariableTarget.Process);

                    PythonEngine.PythonPath = PythonEngine.PythonPath + Path.PathSeparator +
                                            Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
                    PythonEngine.PythonHome = pathToVirtualEnv;
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
