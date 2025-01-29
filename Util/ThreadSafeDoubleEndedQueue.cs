using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Blockgame_OpenTK.Util;

public class ThreadSafeDoubleEndedQueue<T>
{

    private LinkedList<T> _queue = new();
    public int Count => _queue.Count;

    private Mutex _mutex = new();

    public void EnqueueFirst(T value)
    {
        _mutex.WaitOne();
        _queue.AddFirst(value);
        _mutex.ReleaseMutex();
    }

    public void EnqueueLast(T value)
    {
        _mutex.WaitOne();
        _queue.AddLast(value);
        _mutex.ReleaseMutex();
    }

    public bool TryDequeueFirst(out T value)
    {
        _mutex.WaitOne();
        if (_queue.Count > 0)
        {
            value = _queue.FirstOrDefault();
            _queue.RemoveFirst();
            _mutex.ReleaseMutex();
            return true;
        }
        value = default;
        _mutex.ReleaseMutex();
        return false;
    }

    public bool TryDequeueLast(out T value)
    {
        _mutex.WaitOne();
        if (_queue.Count > 0)
        {
            value =  _queue.LastOrDefault();
            _queue.RemoveLast();
            _mutex.ReleaseMutex();
            return true;
        }
        value = default;
        _mutex.ReleaseMutex();
        return false;
    }

}