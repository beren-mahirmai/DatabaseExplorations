using System;
using System.Collections.Generic;
using System.Linq;
using Test.Util;
using DBLib;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class BinarySearchTreeTests
    {
        private DataGen dataGen = new DataGen();

        [Test]
        public void TestAddMultipleElements()
        {
            var tree = new BinarySearchTree();
            List<KeyValuePair<String, String>> dataAdded = dataGen.GenKeyValuePairs(20);
            List<KeyValuePair<String, String>> dataExcluded = dataGen.GenKeyValuePairs(20);
            Assert.AreEqual(0, tree.Count);
            dataAdded.ForEach(x => tree.Add(x.Key, x.Value));
            Assert.AreEqual(20, tree.Count);
            dataAdded.ForEach(x => Assert.IsTrue(tree.Contains(x.Key)));
            dataAdded.ForEach(x => Assert.IsTrue(tree.Get<String>(x.Key) == x.Value));
            dataExcluded.ForEach(x => Assert.IsFalse(tree.Contains(x.Key)));
        }

        [Test]
        public void TestToSortedArray() {
            var tree = new BinarySearchTree();
            List<KeyValuePair<String, String>> dataAdded = dataGen.GenKeyValuePairs(20);
            dataAdded.ForEach(x => tree.Add(x.Key, x.Value));
            KeyValuePair<String, Object>[] sortedData = tree.ToSortedArray();

            // The result should be the expected length
            Assert.AreEqual(20, sortedData.Length);

            // Each input element must be represented
            dataAdded.ForEach(kvpIn => {
                Assert.IsTrue(((String) sortedData.First(y => y.Key == kvpIn.Key).Value) == kvpIn.Value);
            });

            // The elements should all be sorted
            Boolean containsUnsortedElements = Enumerable
                .Range(1, sortedData.Length-1)
                .Any(i => String.Compare(sortedData[i-1].Key, sortedData[i].Key, StringComparison.CurrentCultureIgnoreCase) > 0);
            Assert.False(containsUnsortedElements);
        }
    }
}