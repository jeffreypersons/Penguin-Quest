using System;
using System.Linq;
using System.Collections;
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
    public sealed class CircularBuffer<T> : IEnumerable<T>
    {
        // note index range is inclusive,exclusive
        private int _start;
        private int _end;
        private int _size;
        private readonly T[] _buffer;

        public int Size     => _size;
        public int Capacity => _buffer.Length;
        public T   Front    => this[0];
        public T   Back     => this[_size-1];

        public T this[int index]
        {
            get => _buffer[InternalIndex(index)];
            set => _buffer[InternalIndex(index)] = value;
        }

        
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _size; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public override string ToString() => "[" + string.Join(",", this.Select((T item) => item.ToString())) + "]";


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
            _size  = size;
            _start = 0;
            _end   = capacity == size? 0 : _size;
        }

        /* Reset buffer data without reallocations. */
        public void Clear()
        {
            _size  = 0;
            _start = 0;
            _end   = 0;
        }

        /* Add item to back (tail) of buffer, removing item at front if full. */
        public void PushBack(T item)
        {
            // note that since our upper bound is exclusive we insert at the current end before updating the index
            if (_size == _buffer.Length)
            {
                _buffer[_end] = item;
            }
            else
            {
                _buffer[_end] = item;
                ++_size;
            }
            Increment(ref _end);
        }

        /* Add item to front (head) of buffer, removing item at back if full. */
        public void PushFront(T item)
        {
            // note that since our lower bound is inclusive we update the index before inserting at the new start
            Decrement(ref _start);
            if (_size == _buffer.Length)
            {
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                _buffer[_start] = item;
                ++_size;
            }
        }

        /* Remove item from back (tail) of buffer. */
        public void PopBack()
        {
            if (_size == 0)
            {
                return;
            }

            Decrement(ref _end);
            --_size;
        }

        /* Remove item from front (head) of buffer. */
        public void PopFront()
        {
            if (_size == 0)
            {
                return;
            }

            Increment(ref _start);
            --_size;
        }


        private int InternalIndex(int index)
        {
            if (_size == 0 || index < 0 || index >= _size)
            {
                throw new IndexOutOfRangeException($"Given index={index} outside of range [0, size={_size})");
            }

            int actualIndex = _start + index;
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
