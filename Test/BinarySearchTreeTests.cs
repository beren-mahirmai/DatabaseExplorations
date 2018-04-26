using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using DBLib;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class BinarySearchTreeTests
    {
        Faker DataFaker = new Faker();

        // Generates a list of random strings we can use to test the BloomFilter.
        private List<String> GenData(Int32 size) {
            var list = new List<String>(size);
            for(var i=0; i<size; i++) {
                list.Add(DataFaker.Hacker.Phrase() + DataFaker.Finance.Amount(100, 999, 0));
            }
            return list;
        }

        [Test]
        public void TestAddMultipleElements()
        {
            var tree = new BinarySearchTree<String>(StringComparer.CurrentCultureIgnoreCase);
            List<String> dataAdded = GenData(20);
            List<String> dataExcluded = GenData(20);
            dataAdded.ForEach(x => tree.Add(x));
            dataAdded.ForEach(x => Assert.IsTrue(tree.Contains(x)));
            dataExcluded.ForEach(x => Assert.IsFalse(tree.Contains(x)));
        }

        [Test]
        public void TestToSortedArray() {
            var tree = new BinarySearchTree<String>(StringComparer.CurrentCultureIgnoreCase);
            List<String> dataAdded = GenData(20);
            dataAdded.ForEach(x => tree.Add(x));
            String[] sortedData = tree.ToSortedArray();

            Boolean containsUnsortedElements = Enumerable
                .Range(1, sortedData.Length)
                .Any(i => String.Compare(sortedData[i-1], sortedData[i], StringComparison.CurrentCultureIgnoreCase) > 0);

            Assert.False(containsUnsortedElements);
        }
    }
}