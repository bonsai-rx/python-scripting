using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Bonsai.Expressions;

namespace Bonsai.Scripting.Python
{
    class ModuleNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return context != null;
        }

        static bool IsGroup(IWorkflowExpressionBuilder builder)
        {
            return builder is IncludeWorkflowBuilder || builder is GroupWorkflowBuilder;
        }

        static IEnumerable<ExpressionBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var element = ExpressionBuilder.Unwrap(node.Value);
                yield return element;

                var workflowBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(workflowBuilder))
                {
                    var workflow = workflowBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var groupElement in SelectContextElements(workflow))
                    {
                        yield return groupElement;
                    }
                }
            }
        }

        static bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
        {
            context.Push(source);
            if (source == target)
            {
                return true;
            }

            foreach (var element in SelectContextElements(source))
            {
                var groupBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(groupBuilder) && groupBuilder.Workflow == target)
                {
                    return true;
                }

                if (element is WorkflowExpressionBuilder workflowBuilder)
                {
                    if (GetCallContext(workflowBuilder.Workflow, target, context))
                    {
                        return true;
                    }
                }
            }

            context.Pop();
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                var builderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                if (workflowBuilder != null && builderGraph != null)
                {
                    var callContext = new Stack<ExpressionBuilderGraph>();
                    if (GetCallContext(workflowBuilder.Workflow, builderGraph, callContext))
                    {
                        var names = (from level in callContext
                                     from element in SelectContextElements(level)
                                     let module = ExpressionBuilder.GetWorkflowElement(element) as CreateModule
                                     where module != null
                                     select module.Name)
                                     .Distinct()
                                     .ToList();
                        return new StandardValuesCollection(names);
                    }
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
