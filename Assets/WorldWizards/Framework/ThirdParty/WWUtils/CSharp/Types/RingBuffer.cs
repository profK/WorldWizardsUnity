using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RingBuffer<T> {
    private T[] _storage;
    private int _tail=0;
    private int _head = 0;

    public bool IsEmpty
    {
        get
        {
            return _tail == _head;
        }
    }

    public bool IsFull
    {
        get
        {
            return ((_head + 1) % (_storage.Length)) == _tail;
        }
    }

    public RingBuffer(int buffsz=100)
    {
        _storage = new T[buffsz];
    }

    public void Push(T value)
    {
        int nextPos = (_head + 1) % (_storage.Length);
        if (nextPos == _tail) {  // buffer full
            DoBufferFull();
        }
        _storage[_head] = value;
        _head = nextPos;
    }

    private void DoBufferFull()
    {
        //for now just throw an exception, may wire up expansion later
        throw new InvalidOperationException("Ring buffer over-flow, push dropped");
    }

    public T Pop()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("Ring buffer under-flow, pop dropped");
        } else
        {
            T val = _storage[_tail];
            _tail = (_tail + 1) % (_storage.Length);
            return val;
        }
        
    }
}
