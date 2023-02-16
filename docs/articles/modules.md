# Dynamic Modules

The Python Scripting package allows you to define dynamic modules at runtime. This is useful if you need to isolate specific scripts, or keep track of state variables evolving independently in different parts of the workflow.

To create a new module, you can use the [CreateModule](xref:Bonsai.Scripting.Python.CreateModule) operator together with a [ResourceSubject](xref:Bonsai.Reactive.ResourceSubject):

:::workflow
![CreateModule](~/workflows/create-module.bonsai)
:::

> [!Warning]
> Make sure to always pair the [CreateModule](xref:Bonsai.Scripting.Python.CreateModule) operator together with a [ResourceSubject](xref:Bonsai.Reactive.ResourceSubject) to ensure that your module is correctly destroyed when your workflow terminates.

Once the module is declared, you can now pass it to any of your Python operators to make sure they run their code in the correct scope:

:::workflow
![ShareModule](~/workflows/share-module.bonsai)
:::

> [!Tip]
> You can think of modules as "objects", as they encapsulate all their state variables inside a unique Python scope.

> [!Warning]
> If you do not provide a specific module for a Python scripting operator to run in, it will always run in the global Python main module.