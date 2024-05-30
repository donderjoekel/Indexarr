using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NzbDrone.Core.Concurrency;

public class ConcurrentWork : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly HashSet<Action> _actions;

    private ConcurrentWork(int count)
    {
        _semaphore = new SemaphoreSlim(count);
        _actions = new HashSet<Action>();
    }

    public void AddWork(Action action)
    {
        _actions.Add(action);
    }

    public void Run()
    {
        RunAsync().GetAwaiter().GetResult();
    }

    public async Task RunAsync()
    {
        var tasks = new List<Task>();

        foreach (var action in _actions)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _semaphore.WaitAsync();

                try
                {
                    action();
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }

    public static ConcurrentWork Create<T>(int count, IEnumerable<T> collection, Func<T, Action> callback)
    {
        var concurrentWork = new ConcurrentWork(count);
        foreach (var x in collection)
        {
            concurrentWork.AddWork(callback(x));
        }

        return concurrentWork;
    }

    public static void CreateAndRun<T>(int count, IEnumerable<T> collection, Func<T, Action> callback)
    {
        var concurrentWork = new ConcurrentWork(count);
        foreach (var x in collection)
        {
            concurrentWork.AddWork(callback(x));
        }

        concurrentWork.Run();
        concurrentWork.Dispose();
    }

    public static async Task CreateAndRunAsync<T>(int count, IEnumerable<T> collection, Func<T, Action> callback)
    {
        var concurrentWork = new ConcurrentWork(count);
        foreach (var x in collection)
        {
            concurrentWork.AddWork(callback(x));
        }

        await concurrentWork.RunAsync();
        concurrentWork.Dispose();
    }
}
