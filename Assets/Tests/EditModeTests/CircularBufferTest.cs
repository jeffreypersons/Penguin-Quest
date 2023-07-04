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


        #region CircularBuffer Deletions
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        public void Clear_AllItems(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            circularBuffer.Clear();
            Assert.AreEqual(new char[] { }, circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(circularBuffer.Front, circularBuffer.Back);
        }

        [Test]
        public void PopBack_Empty()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1);
            circularBuffer.PopBack();
            Assert.AreEqual(new int[] { }, circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(0, circularBuffer.Front);
            Assert.AreEqual(0, circularBuffer.Back);
        }

        [Test]
        public void PopFront_Empty()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1, items: new int[] { 0 });
            circularBuffer.PopBack();
            Assert.AreEqual(new int[] { }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }

        [Test]
        public void PopBack_SingleItem()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1, items: new int[] { 1 });
            circularBuffer.PopBack();
            Assert.AreEqual(new int[] { }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }

        [Test]
        public void PopFront_SingleItem()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1, items: new int[] { 0 });
            circularBuffer.PopBack();
            Assert.AreEqual(new int[] { }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }
        #endregion


        #region CircularBuffer Insertions
        [Test]
        public void PushBack_SingleItem()
        {
            CircularBuffer<int> circularBuffer = new(1);
            circularBuffer.PushFront(0);
            Assert.AreEqual(new int[] { 1 }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }

        [Test]
        public void PushFront_SingleItem()
        {
            CircularBuffer<int> circularBuffer = new(1);
            circularBuffer.PushFront(0);
            Assert.AreEqual(new int[] { 1 }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(1, circularBuffer.Front);
            Assert.AreEqual(1, circularBuffer.Back);
        }
        #endregion


        #region CircularBuffer Full State
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void FillAndEmpty_FromBack(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length);
            foreach (var item in items)
            {
                circularBuffer.PushBack(item);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
            foreach (var _ in items)
            {
                circularBuffer.PopBack();
            }
            Assert.AreEqual(new char[] { }, circularBuffer.Items().ToArray());
        }
        
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void FillAndEmpty_FromFront(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length);
            foreach (var item in items)
            {
                circularBuffer.PushFront(item);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
            foreach (var _ in items)
            {
                circularBuffer.PopFront();
            }
            Assert.AreEqual(new char[] { }, items);
        }
        #endregion


        #region CircularBuffer Insertions
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Cycle_AllItems_InOrder_Once(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            for (int i = 0; i < items.Length; i++)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Cycle_AllItems_InReverse_Once(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            for (int i = items.Length-1; i >= 0; i--)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items.Reverse().ToArray(), circularBuffer.Items().ToArray());
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Cycle_AllItems_InOrder_Twice(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            for (int i = 0; i < items.Length; i++)
            {
                circularBuffer.PushFront(items[i]);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
            for (int i = 0; i < items.Length; i++)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
        }
        #endregion

        #region CircularBuffer Shifting
        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Shift_AllItems_Left_Once(params char[] items)
        {
            var target = items[0];
            var expected = items.Skip(0).Append(target).ToArray();

            CircularBuffer<char> circularBuffer = new(items.Length, items);
            circularBuffer.PushBack(target);
            Assert.AreEqual(expected, circularBuffer.Items().ToArray());
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        [TestCase('A', 'B', 'C', 'D')]
        [TestCase('A', 'B', 'C', 'D', 'E')]
        public void Shift_AllItems_Right_Once(params char[] items)
        {
            var target = items[^1];
            var expected = items.SkipLast(0).Prepend(target).ToArray();

            CircularBuffer<char> circularBuffer = new(items.Length, items);
            circularBuffer.PushBack(target);
            Assert.AreEqual(expected, circularBuffer.Items().ToArray());
        }
        #endregion
    }
}
