using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Util;

public class ThreadSafeDoubleEndedQueue<T>
{

    private LinkedList<T> _queue = new();
    public int Count => _queue.Count;

    private Mutex _mutex = new();
    private object _lock = new();

    public void Clear()
    {
        lock (_lock)
        {
            _queue.Clear();
        }
    }
    public void EnqueueBehindFirst(T value)
    {
        lock (_lock)
        {
            if (_queue.Count <= 0) 
            {
                _queue.AddFirst(value);
            } else {
                _queue.AddBefore(_queue.First, value);
            }
        }
    }
    public void EnqueueFirst(T value)
    {
        lock (_lock)
        {
            _queue.AddFirst(value);
        }
    }

    public void EnqueueLast(T value)
    {
        lock (_lock)
        {
            _queue.AddLast(value);
        }
    }

    public bool TryDequeueFirst(out T value)
    {

        lock (_lock)
        {
            if (_queue.Count > 0)
            {
                value = _queue.FirstOrDefault();
                _queue.RemoveFirst();
                return true;
            }
            value = default;
            return false;
        }

    }

    public bool TryDequeueLast(out T value)
    {

        lock (_lock)
        {
            if (_queue.Count > 0)
            {
                value =  _queue.LastOrDefault();
                _queue.RemoveLast();
                return true;
            }
            value = default;
            return false;
        }
        
    }

}