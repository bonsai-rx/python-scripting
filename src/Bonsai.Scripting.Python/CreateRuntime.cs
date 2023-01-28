using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bonsai.Scripting.Python
{
    [DefaultProperty(nameof(PythonHome))]
    public class CreateRuntime : Source<RuntimeManager>
    {
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string PythonHome { get; set; }

        public override IObservable<RuntimeManager> Generate()
        {
            return Observable.Create<RuntimeManager>(observer =>
            {
                var disposable = SubjectManager.ReserveSubject();
                var subscription = disposable.Subject.SubscribeSafe(observer);
                var runtime = new RuntimeManager(PythonHome, disposable.Subject);
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
