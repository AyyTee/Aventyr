using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using Game;
using Ui;

namespace UiTests
{
    [TestFixture]
    public class OrderedSetTests
    {
        [Test]
        public void AddRangeTest0()
        {
            int count = 50;
            var expected = Enumerable.Range(1, count).RandomSubset(count);

            var ordered = new OrderedSet<int>();
            ordered.AddRange(expected);

            Assert.AreEqual(expected, ordered);
        }

        [Test]
        public void AddRangeTest1()
        {
            var array = new int[100];
            var random = new Random(12345);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (int)random.NextLong(0, 50);
            }

            var ordered0 = new OrderedSet<int>();
            ordered0.AddRange(array);

            var ordered1 = new OrderedSet<int>();
            for (int i = 0; i < array.Length; i++)
            {
                ordered1.Add(array[i]);
            }

            Assert.AreEqual(ordered0, ordered1);
        }
    }
}
