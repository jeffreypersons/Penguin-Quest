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
