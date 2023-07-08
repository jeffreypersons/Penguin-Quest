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
            Assert.AreEqual(string.Empty, string.Join(' ', circularBuffer));
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void Construct_SingleItem_BackShouldEqualFront()
        {
            CircularBuffer<char> circularBuffer = new(1, new char[1] { 'A' });
            Assert.AreEqual(circularBuffer.Front, circularBuffer.Back);
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
            Assert.AreEqual(string.Join(' ', items), string.Join(' ', circularBuffer));
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
        #endregion


        #region CircularBuffer Deletions
        [Test]
        public void Pop_Empty_FromBack()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            circularBuffer.PopBack();
            Assert.AreEqual(string.Empty, string.Join(' ', circularBuffer));
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void Pop_Empty_FromFront()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            circularBuffer.PopFront();
            Assert.AreEqual(string.Empty, string.Join(' ', circularBuffer));
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Clear_AllItems(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            circularBuffer.Clear();
            Assert.AreEqual(string.Empty, string.Join(' ', circularBuffer));
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Pop_AllItems_FromBack(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            foreach (var _ in items)
            {
                circularBuffer.PopBack();
            }
            Assert.AreEqual(string.Empty, string.Join(' ', circularBuffer));
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Pop_AllItems_FromFront(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            foreach (var _ in items)
            {
                circularBuffer.PopFront();
            }
            Assert.AreEqual(string.Empty, string.Join(' ', circularBuffer));
            Assert.AreEqual(0, circularBuffer.Size);
        }
        #endregion


        #region CircularBuffer Insertions
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Push_AllItems_FromBack(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length);
            foreach (var item in items)
            {
                circularBuffer.PushBack(item);
            }
            Assert.AreEqual(string.Join(' ', items), string.Join(' ', circularBuffer));
            Assert.AreEqual(items.Length, circularBuffer.Size);
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Push_AllItems_FromFront(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length);
            foreach (var item in items)
            {
                circularBuffer.PushFront(item);
            }
            Assert.AreEqual(string.Join(' ', items), string.Join(' ', circularBuffer));
            Assert.AreEqual(items.Length, circularBuffer.Size);
        }
        #endregion


        #region CircularBuffer Replace All
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void FullState_ReplaceAll_FromFront(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            foreach (var item in items)
            {
                circularBuffer.PushFront(item);
            }
            Assert.AreEqual(string.Join(' ', items), string.Join(' ', circularBuffer));
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void FullState_ReplaceAll_FromBack(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            foreach (var item in items)
            {
                circularBuffer.PushBack(item);
            }
            Assert.AreEqual(string.Join(' ', items), string.Join(' ', circularBuffer));
        }
        #endregion
    }
}
