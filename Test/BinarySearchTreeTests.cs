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
            var tree = new BinarySearchTree<String>(StringComparer.CurrentCultureIgnoreCase);
            List<String> dataAdded = dataGen.GenStrings(20);
            List<String> dataExcluded = dataGen.GenStrings(20);
            Assert.AreEqual(0, tree.Count);
            dataAdded.ForEach(x => tree.Add(x));
            Assert.AreEqual(20, tree.Count);
            dataAdded.ForEach(x => Assert.IsTrue(tree.Contains(x)));
            dataExcluded.ForEach(x => Assert.IsFalse(tree.Contains(x)));
        }

        [Test]
        public void TestToSortedArray() {
            var tree = new BinarySearchTree<String>(StringComparer.CurrentCultureIgnoreCase);
            List<String> dataAdded = dataGen.GenStrings(20);
            dataAdded.ForEach(x => tree.Add(x));
            String[] sortedData = tree.ToSortedArray();

            // The result should be the expected length
            Assert.AreEqual(20, sortedData.Length);

            // Each input element must be represented
            Assert.False(dataAdded.Any(x => !sortedData.Contains(x)));

            // The elements should all be sorted
            Boolean containsUnsortedElements = Enumerable
                .Range(1, sortedData.Length-1)
                .Any(i => String.Compare(sortedData[i-1], sortedData[i], StringComparison.CurrentCultureIgnoreCase) > 0);
            Assert.False(containsUnsortedElements);
        }
    }
}