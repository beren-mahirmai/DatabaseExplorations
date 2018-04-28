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
        private Int64 dataCounter = 0;
        private List<String> GenData(Int32 size) {
            var list = new List<String>(size);
            for(var i=0; i<size; i++) {
                list.Add(DataFaker.Hacker.Phrase() + dataCounter);
                dataCounter++;
            }
            return list;
        }

        [Test]
        public void TestAddMultipleElements()
        {
            var tree = new BinarySearchTree<String>(StringComparer.CurrentCultureIgnoreCase);
            List<String> dataAdded = GenData(20);
            List<String> dataExcluded = GenData(20);
            Assert.AreEqual(0, tree.Count);
            dataAdded.ForEach(x => tree.Add(x));
            Assert.AreEqual(20, tree.Count);
            dataAdded.ForEach(x => Assert.IsTrue(tree.Contains(x)));
            dataExcluded.ForEach(x => Assert.IsFalse(tree.Contains(x)));
        }

        [Test, Ignore("Not done yet")]
        public void TestToSortedArray() {
            var tree = new BinarySearchTree<String>(StringComparer.CurrentCultureIgnoreCase);
            List<String> dataAdded = GenData(20);
            dataAdded.ForEach(x => tree.Add(x));
            String[] sortedData = tree.ToSortedArray();

            // The result should be the expected length
            Assert.AreEqual(20, sortedData.Length);

            // Each input element must be represented
            Assert.False(dataAdded.Any(x => !sortedData.Contains(x)));

            // The elements should all be sorted
            Boolean containsUnsortedElements = Enumerable
                .Range(1, sortedData.Length)
                .Any(i => String.Compare(sortedData[i-1], sortedData[i], StringComparison.CurrentCultureIgnoreCase) > 0);
            Assert.False(containsUnsortedElements);
        }

        public class NumberComparer : IComparer<Int32>
        {
            public int Compare(int x, int y)
            {
                if(x > y) return 1;
                if(x == y) return 0;
                return -1;
            }
        }

        [Test]
        public void AddingDuplicatesDoesNothing() {
            var tree = new BinarySearchTree<Int32>(new NumberComparer());
            tree.Add(5);
            tree.Add(5);
            tree.Add(5);
            Assert.AreEqual(1, tree.Count);
        }

        [TestCase(10, null, null, 5, BinarySearchTree<Int32>.Path.Current)] 
        [TestCase(10, null, null, 15, BinarySearchTree<Int32>.Path.Back)] 
        [TestCase(10, 7, null, 8, BinarySearchTree<Int32>.Path.Current)]
        [TestCase(10, 7, null, 5, BinarySearchTree<Int32>.Path.Lower)]
        [TestCase(10, 7, null, 15, BinarySearchTree<Int32>.Path.Back)]
        [TestCase(10, null, 13, 8, BinarySearchTree<Int32>.Path.Current)]
        [TestCase(10, null, 13, 12, BinarySearchTree<Int32>.Path.Higher)]
        [TestCase(10, 7, 13, 8, BinarySearchTree<Int32>.Path.Current)]     
        [TestCase(10, 7, 13, 5, BinarySearchTree<Int32>.Path.Lower)]  
        [TestCase(10, 7, 13, 12, BinarySearchTree<Int32>.Path.Higher)]     
        [TestCase(10, 7, 13, 15, BinarySearchTree<Int32>.Path.Back)]        
        public void DeterminePathWithBothSubtrees(Int32 nodeValue, Int32? lowNodeValue, Int32? highNodeValue, 
            Int32 testValue, BinarySearchTree<Int32>.Path expectedResult) {

            var tree = new BinarySearchTree<Int32>(new NumberComparer());
            var node = new BinarySearchTree<Int32>.Node(nodeValue);
            node.LowerValues = lowNodeValue != null ? new BinarySearchTree<Int32>.Node((Int32)lowNodeValue) : null;
            node.HigherValues = highNodeValue != null ? new BinarySearchTree<Int32>.Node((Int32)highNodeValue) : null;
            Assert.AreEqual(expectedResult, tree.DeterminePath(node, testValue));
        }
    }
}