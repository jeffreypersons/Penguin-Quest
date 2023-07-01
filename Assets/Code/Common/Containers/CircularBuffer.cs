using System;


namespace PQ.Common.Containers
{
    /*
    Simple memory efficient buffer useful for storing fixed number of items (eg log history).

    Overview
    - A simple memory/cpu/cache friendly efficient data structure that allows adding or removing from either end,
      with a fixed capacity that is never exceeded by its size

    Properties
    - low memory footprint : up-front allocation
    - O(1) lookups         : constant time lookups (whether at back, front, or an index in between)
    - O(1) pop/push        : constant time insertions to front or back of queue

    Notes
    - Implemented as a double-ended queue with a fixed capacity
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
        public T   Front    => _buffer[_frontIndex];        
        public T   Back     => _buffer[_backIndex];

        public T this[int index]
        {
            get
            {
                // note: rely on array for bounds checks instead of adding here,
                // since in this case since most the time this will called in a loop
                return _buffer[InternalIndex(index)];
            }
            set
            {
                _buffer[InternalIndex(index)] = value;
            }
        }

        public CircularBuffer(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException($"Circular buffer cannot have zero or negative capacity, received capacity={capacity}");
            }
            _buffer = new T[capacity];
            Clear();
        }

        /* Reset buffer data without reallocations. */
        public void Clear()
        {
            _size       = 0;
            _frontIndex = 0;
            _backIndex  = 0;
        }

        /* Add item to front (head) of buffer, removing item at back if full. */
        public void PushFront(T item)
        {
            if (_size == _buffer.Length)
            {
                _buffer[_frontIndex] = item;
                Decrement(ref _frontIndex);
                Decrement(ref _backIndex);
            }
            else
            {
                Decrement(ref _frontIndex);
                _buffer[_frontIndex] = item;
                ++_size;
            }
        }

        /* Add item to back (tail) of buffer, removing item at front if full. */
        public void PushBack(T item)
        {
            if (_size == _buffer.Length)
            {
                _buffer[_backIndex] = item;
                Increment(ref _frontIndex);
                Increment(ref _backIndex);
            }
            else
            {
                _buffer[_backIndex] = item;
                Increment(ref _backIndex);
                ++_size;
            }
        }

        /* Remove item from front (head) of buffer. */
        public void PopFront()
        {
            Increment(ref _frontIndex);
            --_size;
        }

        /* Remove item from back (tail) of buffer. */
        public void PopBack()
        {
            Decrement(ref _backIndex);
            --_size;
        }


        private int InternalIndex(int index)
        {
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
