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
        public void PopBack_Empty()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void PopFront_Empty()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1);
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void PopBack_SingleItem()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1, items: new char[] { 'A' });
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void PopFront_SingleItem()
        {
            CircularBuffer<char> circularBuffer = new(capacity: 1, items: new char[] { 'A' });
            circularBuffer.PopBack();
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
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
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
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
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
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
            Assert.AreEqual(Array.Empty<char>(), circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
        }
        #endregion


        #region CircularBuffer Insertions
        [Test]
        public void Push_SingleItem_FromBack()
        {
            var item = 'A';
            CircularBuffer<char> circularBuffer = new(1);
            circularBuffer.PushBack(item);
            Assert.AreEqual(new char[] { item }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1,    circularBuffer.Size);
            Assert.AreEqual(item, circularBuffer.Front);
            Assert.AreEqual(item, circularBuffer.Back);
        }

        [Test]
        public void Push_SingleItem_FromFront()
        {
            var item = 'A';
            CircularBuffer<char> circularBuffer = new(1);
            circularBuffer.PushFront(item);
            Assert.AreEqual(new char[] { item }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1,    circularBuffer.Size);
            Assert.AreEqual(item, circularBuffer.Front);
            Assert.AreEqual(item, circularBuffer.Back);
        }

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
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
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
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
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
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
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
                circularBuffer.PushFront(item);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
        }
        #endregion
    }
}
