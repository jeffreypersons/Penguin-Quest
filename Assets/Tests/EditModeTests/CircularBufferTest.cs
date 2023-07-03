using System;
using System.Linq;
using NUnit.Framework;
using PQ.Common.Containers;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
    {
        [Test]
        public void Construct_InsufficientCapacity_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(0));
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(capacity: 1, items: new int[] { 0, 1 }));
        }

        [Test]
        public void Construct_Empty_SizeShouldBeZero()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1);
            Assert.AreEqual(0, circularBuffer.Size);
        }

        [Test]
        public void Construct_ItemsShouldMatch()
        {
            var items = new int[] { 0, 1 };
            CircularBuffer<int> circularBuffer = new(capacity: items.Length, items);
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
        }


        [Test]
        public void FillAndEmpty_SingleItem()
        {
            CircularBuffer<int> circularBuffer = new(1);
            circularBuffer.PushFront(0);
            Assert.AreEqual(new int[] { 0 }, circularBuffer.Items().ToArray());
            Assert.AreEqual(1, circularBuffer.Size);
            Assert.AreEqual(circularBuffer.Front, circularBuffer.Back);

            circularBuffer.PopBack();
            Assert.AreEqual(new int[] { }, circularBuffer.Items().ToArray());
            Assert.AreEqual(0, circularBuffer.Size);
            Assert.AreEqual(circularBuffer.Front, circularBuffer.Back);
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        public void FillAndEmpty_FromFront(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            foreach (var item in items)
            {
                circularBuffer.PushFront(item);
            }
            Assert.AreEqual(items, circularBuffer.Items().ToArray());
            foreach (var _ in items)
            {
                circularBuffer.PopFront();
            }
            Assert.AreEqual(new char[] { }, items.ToArray());
        }

        [Test]
        [TestCase('A')]
        [TestCase('A', 'B')]
        [TestCase('A', 'B', 'C')]
        public void FillAndEmpty_FromBack(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
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
        public void FillAndEmpty_FullCycle(params char[] items)
        {
            CircularBuffer<char> circularBuffer = new(items.Length, items);
            for (int i = 0; i < items.Length; i++)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items, circularBuffer.Items());
            for (int i = items.Length-1; i >= 0; i--)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items.Reverse().ToArray(), circularBuffer.Items());
        }
    }
}
