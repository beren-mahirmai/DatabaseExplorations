using System;
using NUnit.Framework;
using Bogus;
using DBLib;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    public class BloomFilterTests
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
            var filter = new BloomFilter(10000, 5);
            List<String> dataAdded = GenData(20);
            List<String> dataExcluded = GenData(20);
            dataAdded.ForEach(x => filter.Add(x));
            
            // If an item was added it will alway return true (no false negatives.)
            dataAdded.ForEach(x => Assert.IsTrue(filter.Contains(x)));

            // There is a risk of false positives, but hopefully we set our size large enough.
            dataExcluded.ForEach(x => Assert.IsFalse(filter.Contains(x)));
        }
    }
}