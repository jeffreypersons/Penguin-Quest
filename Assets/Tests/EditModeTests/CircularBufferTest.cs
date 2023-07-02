using NUnit.Framework;
using PQ.Common;
using PQ.Common.Containers;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
    {
        [Test]
        public void EmptyCaseShouldThrowException()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 5);
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
