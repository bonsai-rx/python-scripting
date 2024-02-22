using Bonsai.Expressions;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.Reflection;
using System.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    public class PythonInterpreterLock : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 1);

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
            // var source = arguments.Single();
            // var sourceType = source.Type.GetGenericArguments()[0]; // Get TSource from IObservable<TSource>
            // var factoryParameter = Expression.Parameter(typeof(IObservable<>).MakeGenericType(sourceType));
            Console.WriteLine($"here1 args: {arguments}");
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one input.");
            }
            var sourceType = source.Type.GetGenericArguments()[0]; // Get TSource from IObservable<TSource>
            Console.WriteLine($"here2 sourceType: {source.Type}");
            // var factoryParameter = Expression.Parameter(source.Type);
            var factoryParameter = Expression.Parameter(typeof(IObservable<>).MakeGenericType(sourceType), "factoryParam");
            Console.WriteLine($"here3 factoryParam: {factoryParameter}");
            return BuildWorkflow(arguments, factoryParameter, selectorBody =>
            {
                Console.WriteLine($"here4 selectorBody: {selectorBody}");
                var selector = Expression.Lambda(selectorBody, factoryParameter);
                Console.WriteLine($"here5 selector: {selector}");
                var resultType = selectorBody.Type.GetGenericArguments()[0];
                Console.WriteLine($"here6 resultType: {resultType}");
                // var processMethod = typeof(PythonInterpreterLock).GetMethod(nameof(Process)).MakeGenericMethod(sourceType, resultType);
                // Console.WriteLine($"here7 processMethod: {processMethod}");
                // return Expression.Call(processMethod, source, selector);
                return Expression.Call(GetType(), nameof(Process), new Type[] {sourceType, resultType}, source, selector);
            });
        }

        static IObservable<TResult> Process<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            Console.WriteLine("process");
            return Observable.Defer(() =>
            {
                using (Py.GIL())
                {
                    return selector(source);
                }
            });
        }
    }
}