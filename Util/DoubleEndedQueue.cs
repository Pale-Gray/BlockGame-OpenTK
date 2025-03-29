using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Util;

public class DoubleEndedQueue<T>
{

    private LinkedList<T> _queue = new();
    HashSet<T> _q = new();
    public int Count => _queue.Count;

    public void Clear()
    {
        _queue.Clear();
    }
    public void EnqueueBehindFirst(T value)
    {
        if (_queue.Count <= 0) 
        {
            _queue.AddFirst(value);
        } else {
            _queue.AddBefore(_queue.First, value);
        }
    }
    public void EnqueueFirst(T value)
    {
        _queue.AddFirst(value);
    }

    public void EnqueueLast(T value)
    {
        _queue.AddLast(value);
    }

    public bool TryDequeueFirst(out T value)
    {

        if (_queue.Count > 0)
        {
            value = _queue.ElementAt(0);
            _queue.RemoveFirst();
            return true;
        }
        value = default;
        return false;

    }

    public bool TryDequeueLast(out T value)
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