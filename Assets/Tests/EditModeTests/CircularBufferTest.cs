using System;
using System.Linq;
using NUnit.Framework;
using PQ.Common.Containers;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
    {
        #region CircularBuffer Instantiation
        [Test]
        public void Construct_CapacityLessThanOne_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<char>(0));
        }

        [Test]
        public void Construct_CapacityLessThanSize_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<char>(capacity: 1, items: new char[2]));
        }

        [Test]
        public void Construct_Empty_SizeShouldBeZero()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void Construct_NonEmpty_SizeShouldMatchParams()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 2, items: new char[2]);
            Assert.AreEqual(2, circularBuffer.Size);
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        public void Construct_NonEmpty_ItemsShouldMatchParams(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
        }
        #endregion


        #region CircularBuffer Lookups
        [Test]
        public void Lookup_Empty_AccessingFirst_ShouldThrow()
        {
            CircularBuffer<char> circularBuffer = new(1);
            Assert.Throws<IndexOutOfRangeException>(() => circularBuffer[0].ToString());
        }

        [Test]
        public void Lookup_NonEmpty_AccessingBeyondLength_ShouldThrow()
        {
            CircularBuffer<char> circularBuffer = new(1, new char[1]);
            Assert.Throws<IndexOutOfRangeException>(() => circularBuffer[1].ToString());
        }

        [Test]
        public void Lookup_SingleItem_BackShouldEqualFront()
        {
            CircularBuffer<char> circularBuffer = new(1, new char[1] { 'A' });
            Assert.AreEqual(circularBuffer.Front, circularBuffer.Back);
        }
        #endregion


        #region CircularBuffer Deletions
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        public void Clear_AllItems(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            circularBuffer.Clear();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
        }

        [Test]
        public void PopBack_Empty()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(0, circularBuffer.Front);
            Assert.AreEqual(0, circularBuffer.Back);
        }

        [Test]
        public void PopFront_Empty()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(0, circularBuffer.Front);
            Assert.AreEqual(0, circularBuffer.Back);
        }

        [Test]
        public void PopBack_SingleItem()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1, items: new char[] { 'A' });
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(0, circularBuffer.Front);
            Assert.AreEqual(0, circularBuffer.Back);
        }

        [Test]
        public void PopFront_SingleItem()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1, items: new char[] { 'A' });
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(0, circularBuffer.Front);
            Assert.AreEqual(0, circularBuffer.Back);
        }
        #endregion


        #region CircularBuffer Insertions
        [Test]
        public void PushBack_SingleItem()
        {
            CircularBuffer<char> circularBuffer = new(1);
            circularBuffer.PushBack('A');
            Assert.AreEqual(new char[] { 'A' }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }

        [Test]
        public void PushFront_SingleItem()
        {
            CircularBuffer<char> circularBuffer = new(1);
            circularBuffer.PushFront('A');
            Assert.AreEqual(new char[] { 'A' }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }
        #endregion
    }
}
