using System;
using System.Collections.Generic;
using System.Linq;
using Test.Util;
using DBLib;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class BTreeTests 
    {
        private DataGen dataGen = new DataGen();
        
        [Test]
        public void SimpleTest() {
            var tree = new BTree();
            var key = "ABCD";
            var value = "Mary had a little lamb";
            tree.Set(key, value);
            var contains = tree.Contains(key);
            var x = tree.Get<String>(key);
            Assert.IsTrue(contains);
            Assert.AreEqual(value, x);
        }

        [Test]
        public void TestAddMultipleElements()
        {
            var tree = new BTree();
            List<KeyValuePair<String, String>> dataAdded = dataGen.GenKeyValuePairs(200);
            List<KeyValuePair<String, String>> dataExcluded = dataGen.GenKeyValuePairs(20);
            Assert.AreEqual(0, tree.Count);
            dataAdded.ForEach(x => tree.Set(x.Key, x.Value));
            Assert.AreEqual(200, tree.Count);
            dataAdded.ForEach(x => Assert.IsTrue(tree.Contains(x.Key)));
            dataAdded.ForEach(x => Assert.IsTrue(tree.Get<String>(x.Key) == x.Value));
            dataExcluded.ForEach(x => Assert.IsFalse(tree.Contains(x.Key)));
        } 
    }
}
  
