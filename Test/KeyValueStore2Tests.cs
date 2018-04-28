using System;
using NUnit.Framework;
using DBLib;
using Test.Util;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class KeyValueStore2Tests
    {
        private DataGen dataGen = new DataGen();
        private KeyValueStore2 Store = new KeyValueStore2();

        [SetUp]
        public void SetUp() {
            Store.Initialize();
        }

        [TearDown]
        public void TearDown() {
            Store.Clear();
        }

        [Test]
        public void ReadWriteMultipleStrings() {
            
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(4);
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }

            // Attempt to read from the store and see if we get the correct value for each key.
            foreach(KeyValuePair<String, String> kvp in data) {
                String testValue = Store.Get<String>(kvp.Key);
                Assert.AreEqual(kvp.Value, testValue);
            }
        }

        [Test]
        public void ReplacesExistingValue() {
            // Add test data to the store
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(4);
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }

            // Replace one of the values with a new value.
            String newValue = dataGen.GenString();
            Store.Set(data[1].Key, newValue);
            
            // Verify we retrieve the new value, not the original.
            String testValue = Store.Get<String>(data[1].Key);
            Assert.AreEqual(newValue, testValue);
        }

        [Test]
        public void ThrowsIfKeyNotFoundInPopulatedDatabase() {
            IEnumerable<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(4);
            String unusedKey = dataGen.GenWord();

            // Add each pair to the store.
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }

            Assert.Throws<Exception>(() => {
                Store.Get<String>(unusedKey);
            });
        }

        [Test]
        public void ThrowsIfKeyNotFoundInEmptyDatabase() {
            Assert.Throws<Exception>(() => {
                Store.Get<String>("DummyKey");
            });
        }

        [Test]
        public void SummaryOfMixedMultiUseKeys() {
            // Add test data to the store.
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(3);
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }

            // Leave the first key alone, so it will have one entry in the file.
            // The second entry will get overwritten once, so it has 2 entries.
            // The third will be overwritten twice, so it will have 3 entries.
            Store.Set(data[1].Key, dataGen.GenString());
            Store.Set(data[2].Key, dataGen.GenString());
            Store.Set(data[2].Key, dataGen.GenString());

            // Get the stats and verify we see 3 entries, and the expected counts
            // appear.  Since we know the keys, we can calculate each has and make
            // sure the expected counts appear.
            Dictionary<Int32, Int32> stats = Store.GetStats();
            Assert.AreEqual(3, stats.Count);
            Assert.AreEqual(stats[data[0].Key.GetHashCode()], 1);
            Assert.AreEqual(stats[data[1].Key.GetHashCode()], 2);
            Assert.AreEqual(stats[data[2].Key.GetHashCode()], 3);
        }

        [Test]
        public void CompactData() {
            // Add test data to the store.
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(3);
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }

            // Change each value at least once, but keep the final value of each.
            String finalA = dataGen.GenString();
            String finalB = dataGen.GenString();
            String finalC = dataGen.GenString();

            Store.Set(data[1].Key, dataGen.GenString());
            Store.Set(data[2].Key, dataGen.GenString());
            Store.Set(data[2].Key, dataGen.GenString());

            Store.Set(data[0].Key, finalA);
            Store.Set(data[1].Key, finalB);
            Store.Set(data[2].Key, finalC);

            // Compact the database, then only the most recent values should remain.
            Store.Compact();

            // Check the state of the database, there should be one entry for each
            // key which contains the most recent value.
            Dictionary<Int32, Int32> stats = Store.GetStats();
            Assert.AreEqual(3, stats.Count);
            Assert.AreEqual(stats[data[0].Key.GetHashCode()], 1);
            Assert.AreEqual(stats[data[1].Key.GetHashCode()], 1);
            Assert.AreEqual(stats[data[2].Key.GetHashCode()], 1);
            Assert.AreEqual(finalA, Store.Get<String>(data[0].Key));
            Assert.AreEqual(finalB, Store.Get<String>(data[1].Key));
            Assert.AreEqual(finalC, Store.Get<String>(data[2].Key));
        }

        [Test]
        public void ContainsKeyWithKnownKey() {
            // Add test data to the store.
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(3);
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }
            Assert.IsTrue(Store.ContainsKey(data[1].Key));
        }

        [Test]
        public void ContainsKeyWithMissingKey() {
            // Add test data to the store.
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(3);
            foreach(KeyValuePair<String, String> kvp in data) {
                Store.Set(kvp.Key, kvp.Value);
            }
            Assert.IsFalse(Store.ContainsKey(dataGen.GenWord()));
        }

        [Test, Explicit]
        // NOTE: To see the console output run "dotnet test -v d" for detailed output
        public void MeasurePerformance() {
            Int32 numKeys = 500;
            List<KeyValuePair<String, String>> data = dataGen.GenKeyValuePairs(numKeys);

            // Fill the database with some values writen only in early passes so a read has to 
            // go clear back to the beginning of the file, while others get written to at the end.
            Int32 writeIterations = 0;
            DateTime writeStart = DateTime.Now;
            for(var i=0; i<data.Count; i++) {
                for(var j=0; j<=i; j++) {
                    Store.Set(data[i].Key, dataGen.GenString());
                    writeIterations++;
                }
            }
            DateTime writeEnd = DateTime.Now;
            TimeSpan writeDuration = writeEnd - writeStart;
            Console.WriteLine($"Rows written (across {numKeys} keys): {writeIterations}");
            Console.WriteLine($"Total Write Time: {writeDuration.TotalMilliseconds}");
            Double averageWriteTime = writeDuration.TotalMilliseconds / writeIterations;
            Console.WriteLine($"Average Write Time: {averageWriteTime}");
            
            // Now read each expected key and see how long it takes.  Remember, the seek
            // will have to go through the whole file in the worst case.
            Int32 minReadTime = 100000;
            Int32 maxReadTime = 0;
            DateTime totalReadStart = DateTime.Now;
            data.ForEach(kvp => {
                DateTime individualReadStart = DateTime.Now;
                Store.Get<String>(kvp.Key);
                DateTime individualReadEnd = DateTime.Now;
                Int32 individualReadTime = Convert.ToInt32((individualReadEnd - individualReadStart).TotalMilliseconds);
                if(individualReadTime < minReadTime) minReadTime = individualReadTime;
                if(individualReadTime > maxReadTime) maxReadTime = individualReadTime;
            });
            DateTime totalReadEnd = DateTime.Now;
            Int32 totalReadTime = Convert.ToInt32((totalReadEnd - totalReadStart).TotalMilliseconds);
            Console.WriteLine($"Minimum read time: {minReadTime}");
            Console.WriteLine($"Maximum read time: {maxReadTime}");
            Console.WriteLine($"Total read time:   {totalReadTime}");
            Console.WriteLine($"Average read time: {totalReadTime / numKeys}");
            
            DateTime startContainsKey = DateTime.Now;
            Boolean contained = Store.ContainsKey("ABCDEFGHIJKLMNOPQRSTUVWXYZ"); // watch out, dataGen.GenWord() can produce repeats!
            DateTime endContainsKey = DateTime.Now;
            Assert.IsFalse(contained);
            Console.WriteLine($"Total ContainsKey() time for missing key: {(endContainsKey - startContainsKey).TotalMilliseconds}");

            DateTime startCompact = DateTime.Now;
            Store.Compact();
            DateTime endCompact = DateTime.Now;
            Console.WriteLine($"Total time to compact: {(endCompact - startCompact).TotalMilliseconds}");
        }
    }
}