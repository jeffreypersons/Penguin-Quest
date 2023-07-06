using System;
using System.Linq;
using System.Collections.Generic;


namespace PQ.Common.Containers
{
    /*
    Simple memory efficient buffer useful for storing fixed number of items (eg log history).

    
    Implemented as a double-ended queue with a fixed capacity, providing O(1) lookups and pop/push.

    Overview
    - A simple memory/cpu/cache friendly efficient data structure that allows adding or removing from either end,
      with a fixed capacity that is never exceeded by its size

    Properties
    - O(1) lookups  : constant time lookups (whether at back, front, or an index in between)
    - O(1) pop/push : constant time insertions to front or back of queue

    Notes
    - To avoid allocations from IEnumerable, we expose an indexer and size    
    - No erasure of previous data, everything is handled internally with indices    
    - Empty size is permitted (avoids edge cases when popping)
    */
    public sealed class CircularBuffer<T>
    {
        private readonly T[] _buffer;

        private int _size;
        private int _frontIndex;
        private int _backIndex;

        public int Size     => _size;
        public int Capacity => _buffer.Length;
        public T   Front    => this[0];
        public T   Back     => this[_size-1];

        public T this[int index]
        {
            get => _buffer[InternalIndex(index)];
            set => _buffer[InternalIndex(index)] = value;
        }

        public IEnumerable<T> Items()
        {
            for (int i = 0; i < _size; i++)
            {
                yield return this[i];
            }
        }

        public override string ToString() => "[" + string.Join(",", Items().Select((T item) => item.ToString())) + "]";


        public CircularBuffer(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentException($"Capacity must be >= 1 - received capacity={capacity}");
            }
            _buffer = new T[capacity];
            Clear();
        }

        public CircularBuffer(int capacity, T[] items) : this(capacity)
        {
            int size = items.Length;
            if (capacity < size)
            {
                throw new ArgumentException($"Capacity must be >= size - received capacity={capacity}, size={size}");
            }
            Array.Copy(items, _buffer, size);
            _size = size;
            _frontIndex = 0;
            _backIndex = size-1;
        }

        /* Reset buffer data without reallocations. */
        public void Clear()
        {
            _size       = 0;
            _frontIndex = 0;
            _backIndex  = 0;
        }

        /* Add item to back (tail) of buffer, removing item at front if full. */
        public void PushBack(T item)
        {
            if (_size == _buffer.Length)
            {
                Decrement(ref _backIndex);
                --_size;
            }

            _buffer[_frontIndex] = item;
            Increment(ref _backIndex);
            ++_size;
        }

        /* Add item to front (head) of buffer, removing item at back if full. */
        public void PushFront(T item)
        {
            if (_size == _buffer.Length)
            {
                Increment(ref _frontIndex);
                --_size;
            }

            _buffer[_frontIndex] = item;
            Decrement(ref _frontIndex);
            ++_size;
        }

        /* Remove item from back (tail) of buffer. */
        public void PopBack()
        {
            if (_size != 0)
            {
                Decrement(ref _backIndex);
                --_size;
            }
        }

        /* Remove item from front (head) of buffer. */
        public void PopFront()
        {
            if (_size != 0)
            {
                Increment(ref _frontIndex);
                --_size;
            }
        }


        private int InternalIndex(int index)
        {
            if (_size == 0 || index < 0 || index >= _size)
            {
                throw new IndexOutOfRangeException($"Given index={index} outside of range [0, size={_size})");
            }
            int actualIndex = _frontIndex + index;
            if (actualIndex >= _buffer.Length)
            {
                actualIndex -= _buffer.Length;
            }
            return actualIndex;
        }

        private void Increment(ref int index)
        {
            if (++index == _buffer.Length)
            {
                index = 0;
            }
        }

        private void Decrement(ref int index)
        {
            if (index == 0)
            {
                index = _buffer.Length;
            }
            --index;
        }
    }
}
