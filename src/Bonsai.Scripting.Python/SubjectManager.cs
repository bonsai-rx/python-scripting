using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Bonsai.Scripting.Python
{
    class SubjectManager
    {
        static Tuple<ReplaySubject<RuntimeManager>, RefCountDisposable> runtimeSubject;
        static readonly object subjectLock = new object();

        internal static SubjectDisposable ReserveSubject()
        {
            lock (subjectLock)
            {
                if (runtimeSubject == null)
                {
                    var subject = new ReplaySubject<RuntimeManager>(2);
                    var dispose = Disposable.Create(() =>
                    {
                        subject.Dispose();
                        runtimeSubject = null;
                    });

                    var refCount = new RefCountDisposable(dispose);
                    runtimeSubject = Tuple.Create(subject, refCount);
                    return new SubjectDisposable(subject, refCount);
                }

                return new SubjectDisposable(runtimeSubject.Item1, runtimeSubject.Item2.GetDisposable());
            }
        }

        internal sealed class SubjectDisposable : IDisposable
        {
            IDisposable resource;

            public SubjectDisposable(ISubject<RuntimeManager> subject, IDisposable disposable)
            {
                Subject = subject ?? throw new ArgumentNullException(nameof(subject));
                resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
            }

            public ISubject<RuntimeManager> Subject { get; private set; }

            public void Dispose()
            {
                lock (subjectLock)
                {
                    if (resource != null)
                    {
                        resource.Dispose();
                        resource = null;
                    }
                }
            }
        }
    }
}
