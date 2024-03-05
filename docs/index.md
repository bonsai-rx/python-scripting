# Introduction

The Python Scripting package is a [Bonsai](https://bonsai-rx.org/) interface for the [Python](https://www.python.org/) programming language. It uses [Python.NET](https://pythonnet.github.io/pythonnet/) under the hood to provide a seamless integration between the CPython runtime and data streaming in a Bonsai workflow.

You can use the Python Scripting package to run native Python scripts, import any module available to Python, and read or write to any variable in a named scope. The package is designed to work with any Python version from 3.7 onwards, and you can also use it in combination with [Python virtual environments](https://docs.python.org/3/tutorial/venv.html) to fully isolate your project dependencies.

## How to install

1. Download [Bonsai](https://bonsai-rx.org/).
2. From the package manager, search and install the **Bonsai - Python Scripting** package.

## Create a Python environment

In addition to the Python Scripting package you need to have a version of [Python](https://www.python.org/) installed in your system. We recommend installing the official distributions and using `venv` to create virtual environments to run your specific projects.

To create a virtual environment you can run the following command from inside the folder where you want to install the environment:

```ps
python -m venv example-env
```

## Create a Python runtime

The [CreateRuntime](xref:Bonsai.Scripting.Python.CreateRuntime) node launches the Python kernel and creates the main global scope for running Python scripts. You must set the [PythonHome](xref:Bonsai.Scripting.Python.CreateRuntime.PythonHome) property to the environment folder (or your Python home directory) before starting the workflow:

:::workflow
![CreateRuntime](~/workflows/create-runtime.bonsai)
:::

Alternatively, you can activate the virtual environment *before* starting Bonsai. In this case you do not need to specify the Python home directory.

> [!Tip]
> You can set the value of the [**ScriptPath**](xref:Bonsai.Scripting.Python.CreateRuntime.ScriptPath) property to specify a main script that will be run when the Python runtime is initialized. This script can be used to import all the main modules to be used in your workflow, or initialize any state variables to be manipulated during execution.

## Run Python code

The [Eval](xref:Bonsai.Scripting.Python.Eval) operator can be used anywhere in your workflow to interact with the Python runtime, just as if you were writing code in the Python REPL:

:::workflow
![HelloPython](~/workflows/hello-python.bonsai)
:::

You can also use the [Exec](xref:Bonsai.Scripting.Python.Exec) operator to run Python statements dynamically, for example to import new modules:

:::workflow
![HelloPython](~/workflows/exec-eval.bonsai)
:::

## Read and write state variables

To interface with Python state variables, you can use the [Get](xref:Bonsai.Scripting.Python.Get) and [Set](xref:Bonsai.Scripting.Python.Set) operators:

:::workflow
![GetSet](~/workflows/get-set.bonsai)
:::

> [!Warning]
> All the operators in the Python Scripting package run under the Python [Global Interpreter Lock](https://docs.python.org/3/glossary.html#term-global-interpreter-lock). This means that although execution of Python code can be triggered asynchronously anywhere in the workflow, there will be a bottleneck when accessing the interpreter. Because of this, we currently do not recommend running large number of parallel calls to Python.

## Scripting Extensions

Custom script files can be executed when creating the Python runtime with [CreateRuntime](xref:Bonsai.Scripting.Python.CreateRuntime) or when creating a new module with [CreateModule](xref:Bonsai.Scripting.Python.CreateModule).

To edit script files we recommend [Visual Studio Code](https://code.visualstudio.com/) and the [Python extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-python.python). If both your virtual environment and your custom Python scripts are placed relative to your main workflow, VS Code can be launched from the Bonsai IDE and will be able to correctly pick up the right environment when editing the scripts.

> [!Warning]
> These docs are under active development, feel free to contribute by either [raising an issue](https://github.com/bonsai-rx/python-scripting/issues) or following the links to **Edit this page**.