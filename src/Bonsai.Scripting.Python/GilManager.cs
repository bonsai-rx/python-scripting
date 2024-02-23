using Python.Runtime;
using System;

public sealed class GilManager
{
    private static readonly Lazy<GilManager> _instance = new Lazy<GilManager>(() => new GilManager());
    public static GilManager Instance => _instance.Value;

    private GilManager()
    {
    }

    public void ExecuteWithGil(Action action)
    {
        using (Py.GIL()) // Acquire GIL
        {
            action();
        }
    }
}