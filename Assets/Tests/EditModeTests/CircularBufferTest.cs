using System;
using NUnit.Framework;
using PQ.Common.Containers;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
    {
        [Test]
        [TestCase( 0)]
        [TestCase(-1)]
        public void ThrowExceptionIfNoCapacity(int capacity)
        {
            CircularBuffer<string> circularBuffer = new(capacity);
            Assert.Throws<Exception>(() => new CircularBuffer<char>(capacity));
        }

        [Test]
        [TestCase("A", "B", "C", "D", "E")]
        public void FillToCapacityAndRemove(params string[] items)
        {
            CircularBuffer<string> circularBuffer = new(capacity: items.Length);
            foreach (var item in items)
            {
                circularBuffer.PushBack(item);
            }
            foreach (var item in items)
            {
                circularBuffer.PopBack();
            }

            Assert.Throws<Exception>(() => new string(circularBuffer[1]));
            Assert.Throws<Exception>(() => new string(circularBuffer.Back));
            Assert.Throws<Exception>(() => new string(circularBuffer.Front));
        }

        [Test]
        [TestCase(-1)]
        public void CycleThroughFullBuffer()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 5);
        }

        [Test]
        public void PushAndPopThroughHalfFullBuffer()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 5);
        }
    }
}
