﻿using System;
using System.Collections.Generic;
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

        internal RuntimeManager(string path, IObserver<RuntimeManager> observer)
        {
            runtimeScheduler = new EventLoopScheduler();
            runtimeObserver = observer;
            Schedule(() =>
            {
                Initialize(path);
                threadState = PythonEngine.BeginAllowThreads();
                observer.OnNext(this);
            });
        }

        internal static IObservable<RuntimeManager> RuntimeSource { get; } = Observable.Using(
            () => SubjectManager.ReserveSubject(),
            disposable => disposable.Subject)
            .Take(1);

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
                    PythonEngine.EndAllowThreads(threadState);
                    PythonEngine.Shutdown();
                }
                runtimeScheduler.Dispose();
            });
        }
    }
}
