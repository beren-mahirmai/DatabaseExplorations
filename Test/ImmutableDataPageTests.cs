using System;
using System.Collections.Generic;
using System.Linq;
using Test.Util;
using DBLib;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class ImmutableDataPageTests
    {
        private DataGen dataGen = new DataGen();

        [Test]
        public void TestAddMultipleElements()
        {
            Int32 testSize = 100;
            var tree = new BinarySearchTree();
            List<KeyValuePair<String, String>> dataAdded = dataGen.GenKeyValuePairs(testSize);
            List<KeyValuePair<String, String>> dataExcluded = dataGen.GenKeyValuePairs(testSize);
            dataAdded.ForEach(x => tree.Set(x.Key, x.Value));
            var dataPage = new ImmutableDataPage(tree);

            Assert.AreEqual(testSize, dataPage.Count);
            dataAdded.ForEach(x => Assert.AreEqual(x.Value, dataPage.Get<String>(x.Key)));
            dataExcluded.ForEach(x => Assert.AreEqual(null, dataPage.Get<String>(x.Key)));
        }
    }
}