using System;


namespace PQ._Experimental.Physics
{
    /*
    Simple memory efficient buffer useful for storing fixed number of items (eg log history).
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
                if (index >= _size)
                {
                    throw new IndexOutOfRangeException($"Given index={index} is outside range [0,size={_size})");
                }
                return _buffer[InternalIndex(index)];
            }
            set
            {
                if (index >= _size)
                {
                    throw new IndexOutOfRangeException($"Given index={index} is outside range [0,size={_size})");
                }
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


        public void Clear()
        {
            _size       = 0;
            _frontIndex = 0;
            _backIndex  = 0;
        }

        public void PushFront(T item)
        {
            if (_size == _buffer.Length)
            {
                Decrement(ref _frontIndex);
                Decrement(ref _backIndex);
                _buffer[_frontIndex] = item;
            }
            else
            {
                Decrement(ref _frontIndex);
                _buffer[_frontIndex] = item;
                ++_size;
            }
        }

        public void PushBack(T item)
        {
            if (_size == _buffer.Length)
            {
                Increment(ref _frontIndex);
                Increment(ref _backIndex);
                _buffer[_backIndex] = item;
            }
            else
            {
                Increment(ref _backIndex);
                _buffer[_backIndex] = item;
                ++_size;
            }
        }

        public void PopFront()
        {
            Increment(ref _frontIndex);
            --_size;
        }

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
