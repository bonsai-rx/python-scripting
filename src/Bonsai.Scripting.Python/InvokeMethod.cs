using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using Bonsai.Expressions;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that invokes a method contained in the specified
    /// Python module.
    /// </summary>
    [Description("Invokes a method contained in the specified Python module.")]
    public class InvokeMethod : SingleArgumentExpressionBuilder
    {
        static readonly MethodInfo ToPython = typeof(ConverterExtension).GetMethod(nameof(ConverterExtension.ToPython));

        /// <summary>
        /// Gets or sets the name of the Python module containing the method to invoke.
        /// </summary>
        [TypeConverter(typeof(ModuleNameConverter))]
        [Description("The name of the Python module containing the method to invoke.")]
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the name of the method to invoke.
        /// </summary>
        [Description("The name of the method to invoke.")]
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets a string used to select the input element members that will
        /// be used as arguments for method invocation.
        /// </summary>
        [Description("The inner properties that will be selected as arguments for method invocation.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ArgumentSelector { get; set; } = ExpressionHelper.ImplicitParameterName;

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(parameterType);
            var selectedArguments = string.IsNullOrEmpty(ArgumentSelector) ? Enumerable.Empty<Expression>() : ExpressionHelper
                .SelectMembers(parameter, ArgumentSelector)
                .Select(argument => Expression.Call(ToPython, Expression.Convert(argument, typeof(object))));
            var argsSelectorBody = Expression.NewArrayInit(typeof(PyObject), selectedArguments.ToArray());
            var argsSelector = Expression.Lambda(argsSelectorBody, parameter);
            return Expression.Call(Expression.Constant(this), nameof(Process), new[] { parameterType }, source, argsSelector);
        }

        /// <summary>
        /// Invokes a method contained in the specified Python module whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to invoke the method.
        /// </param>
        /// <param name="selector">
        /// A transform function used to extract the method arguments from the
        /// notification value.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="PyObject"/> handles representing the result
        /// of invoking the method.
        /// </returns>
        public IObservable<PyObject> Process<TSource>(IObservable<TSource> source, Func<TSource, PyObject[]> selector)
        {
            return RuntimeManager.RuntimeSource.SelectMany(runtime =>
            {
                var module = runtime.Resources.Load<PyObject>(ModuleName);
                return source.Select(value =>
                {
                    using (Py.GIL())
                    {
                        return module.InvokeMethod(MethodName, selector(value));
                    }
                });
            });
        }
    }
}
