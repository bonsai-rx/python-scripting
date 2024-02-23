using Bonsai.Expressions;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.Reflection;
using System.Linq;
using Python.Runtime;
using System.Reactive;

namespace Bonsai.Scripting.Python
{
    public class PythonInterpreterLock : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 1);
        private static readonly object _lock = new object();

        public PythonInterpreterLock()
            : this(new ExpressionBuilderGraph())
        {
        }

        public PythonInterpreterLock(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        public override Range<int> ArgumentRange => argumentRange;

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one input.");
            }
            var sourceType = source.Type.GetGenericArguments()[0]; // Get TSource from IObservable<TSource>
            var factoryParameter = Expression.Parameter(typeof(IObservable<>).MakeGenericType(sourceType), "factoryParam");

            return BuildWorkflow(arguments, factoryParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, factoryParameter);
                var resultType = selectorBody.Type.GetGenericArguments()[0];
                return Expression.Call(GetType(), nameof(Process), new Type[] {sourceType, resultType}, source, selector);
            });
        }

        static IObservable<TResult> Process<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            var gilProtectedSource = Observable.Create<TSource>(observer =>
            {
                var sourceObserver = Observer.Create<TSource>(
                    value =>
                    {
                        using (Py.GIL())
                        {
                            observer.OnNext(value); //locking around downstream effects
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(observer);
            });

            return selector(gilProtectedSource);
        }
    }
}