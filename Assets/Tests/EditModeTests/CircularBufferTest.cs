using System;
using NUnit.Framework;
using PQ.Common.Containers;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
    {
        [Test]
        public void ThrowExceptionIfNoCapacity()
        {
            CircularBuffer<string> circularBuffer = new(capacity: 0);
            Assert.Throws<Exception>(() => new CircularBuffer<char>(0));
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
