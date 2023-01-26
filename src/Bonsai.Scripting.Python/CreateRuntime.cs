using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bonsai.Scripting.Python
{
    public class CreateRuntime : Source<RuntimeManager>
    {
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string PythonPath { get; set; }

        public override IObservable<RuntimeManager> Generate()
        {
            return Observable.Create<RuntimeManager>(observer =>
            {
                var disposable = SubjectManager.ReserveSubject();
                var subscription = disposable.Subject.SubscribeSafe(observer);
                var runtime = new RuntimeManager(PythonPath, disposable.Subject);
                return new CompositeDisposable
                {
                    subscription,
                    disposable,
                    runtime
                };
            });
        }
    }
}
