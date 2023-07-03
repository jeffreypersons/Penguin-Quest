using System;
using System.Linq;
using NUnit.Framework;
using PQ.Common.Containers;


namespace PQ.Tests.EditMode
{
    public class CircularBufferTest
    {
        [Test]
        public void Construct_LessThanOneCapacity_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(0));
        }

        [Test]
        public void Construct_InsufficientCapacity_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(capacity: 1, items: new int[] { 0, 1 }));
        }

        [Test]
        public void Construct_Empty_SizeShouldBeZero()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1);
            Assert.AreEqual(circularBuffer.Size, 0);
        }

        [Test]
        public void Construct_SingleItem_ShouldBeAtFrontAndBack()
        {
            CircularBuffer<int> circularBuffer = new(capacity: 1, items: new int[] { 0 });
            Assert.AreEqual(circularBuffer.Size, 1);
            Assert.AreEqual(circularBuffer.Front, circularBuffer.Back);
        }

        [Test]
        public void Construct_ItemsShouldMatch()
        {
            var items = new int[] { 0, 1 };
            CircularBuffer<int> circularBuffer = new(capacity: items.Length, items);
            Assert.AreEqual(circularBuffer.Items(), items.AsEnumerable());
        }


        [Test]
        public void PushAndPop_SingleItem()
        {
            CircularBuffer<int> circularBuffer = new(1);
            circularBuffer.PushFront(0);
            Assert.AreEqual(circularBuffer.Items().ToArray(), new int[] { 0 });

            circularBuffer.PopBack();
            Assert.AreEqual(circularBuffer.Items().ToArray(), new int[] { });
        }

        [Test]
        [TestCase("")]
        [TestCase("A")]
        [TestCase("A B")]
        [TestCase("A B C")]
        public void FillAndEmpty_FromFront(params string[] items)
        {
            CircularBuffer<string> circularBuffer = new(items.Length, items);
            foreach (var item in items)
            {
                circularBuffer.PushFront(item);
            }
            foreach (var _ in items)
            {
                circularBuffer.PopFront();
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("A")]
        [TestCase("A B")]
        [TestCase("A B C")]
        public void FillAndEmpty_FromBack(params string[] items)
        {
            CircularBuffer<string> circularBuffer = new(items.Length, items);
            foreach (var item in items)
            {
                circularBuffer.PushBack(item);
            }
            foreach (var _ in items)
            {
                circularBuffer.PopBack();
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("A")]
        [TestCase("A B")]
        [TestCase("A B C")]
        public void CycleThroughFullBuffer(params string[] items)
        {
            CircularBuffer<string> circularBuffer = new(items.Length, items);
            for (int i = 0; i < items.Length; i++)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items.ToArray(), circularBuffer.Items());
            for (int i = items.Length-1; i >= 0; i--)
            {
                circularBuffer.PushBack(items[i]);
            }
            Assert.AreEqual(items.Reverse().ToArray(), circularBuffer.Items());
        }
    }
}
