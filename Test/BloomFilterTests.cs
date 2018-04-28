using System;
using NUnit.Framework;
using Test.Util;
using DBLib;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class BloomFilterTests
    {
        private DataGen dataGen = new DataGen();

        [Test]
        public void TestAddMultipleElements()
        {
            var filter = new BloomFilter(10000, 5);
            List<String> dataAdded = dataGen.GenStrings(20);
            List<String> dataExcluded = dataGen.GenStrings(20);
            dataAdded.ForEach(x => filter.Add(x));
            
            // If an item was added it will alway return true (no false negatives.)
            dataAdded.ForEach(x => Assert.IsTrue(filter.Contains(x)));

            // There is a risk of false positives, but hopefully we set our size large enough.
            dataExcluded.ForEach(x => Assert.IsFalse(filter.Contains(x)));
        }
    }
}