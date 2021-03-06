using System;
using NUnit.Framework;
using DBLib;
using Test.Util;
using System.Collections.Generic;
using DBLib.Exceptions;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class KeyValueStore3Tests
    {
        private DataGen dataGen = new DataGen();
        private KeyValueStore3 Store = new KeyValueStore3();

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

            Assert.Throws<DataNotFoundException>(() => {
                Store.Get<String>(unusedKey);
            });
        }

        [Test]
        public void ThrowsIfKeyNotFoundInEmptyDatabase() {
            Assert.Throws<DataNotFoundException>(() => {
                Store.Get<String>("DummyKey");
            });
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

        [Test]
        public void FindKeyInStoredDataPage() { 
            String key = dataGen.GenWord();
            String value = dataGen.GenString();

            Store.Set(key, value);
            for(var i = 0; i < 2000; i++) {
                Store.Set($"x{i}x", $"y{i}y");
            }

            Assert.AreEqual(value, Store.Get<String>(key));
        }

        [Test]
        public void FindKeyInMiddleStoredDataPage() {
            String key = dataGen.GenWord();
            String value = dataGen.GenString();

            for(var i = 0; i < 3000; i++) {
                Store.Set($"x{i}x", $"y{i}y");
            }
            Store.Set(key, value);
            for(var i = 4000; i < 7000; i++) {
                Store.Set($"x{i}x", $"y{i}y");
            }

            Assert.AreEqual(value, Store.Get<String>(key));
        }

        [Test]
        public void CountMultipleInstances() {
            String key = dataGen.GenWord();
            String valueA = dataGen.GenString();
            String valueB = dataGen.GenString();
            String valueC = dataGen.GenString();
            String valueD = dataGen.GenString();

            // Set the value three different times with a lot of dummy data
            // in between.  This way the data gets divided between different 
            // data pages.
            Store.Set(key, valueA);
            for(var i = 0; i < 3000; i++) {
                Store.Set($"x{i}x", $"y{i}y");
            }
            Store.Set(key, valueB);
            for(var i = 4000; i < 5500; i++) {
                Store.Set($"x{i}x", $"y{i}y");
            }
            Store.Set(key, valueC);
            for(var i = 5500; i < 7000; i++) {
                Store.Set($"x{i}x", $"y{i}y");
            }
            Store.Set(key, valueD);

            Assert.IsTrue(Store.ContainsKey(key));
            Assert.AreEqual(4, Store.CountInstances(key));
            Assert.IsTrue(Store.ContainsKey("x4444x"));
            Assert.AreEqual(1, Store.CountInstances("x4444x"));
            Assert.IsFalse(Store.ContainsKey("This Key Doesn't Exist"));
            Assert.AreEqual(0, Store.CountInstances("This Key Doesn't Exist"));
        }

        [Test]
        public void CompactWorks() {

            // Set a lot of data so it gets divided between different 
            // data pages.
            for(var i = 0; i < 9000; i++) {
                Store.Set($"x{i}x", $"a{i}a");
            }
            for(var i = 0; i < 9000; i++) {
                Store.Set($"x{i}x", $"b{i}b");
            }
            for(var i = 0; i < 9000; i++) {
                Store.Set($"x{i}x", $"c{i}c");
            }

            Store.Compact();
            
            Assert.AreEqual(1, Store.CountInstances("x2000x"));
            Assert.AreEqual("c2000c", Store.Get<String>("x2000x"));
            Assert.AreEqual(1, Store.CountInstances("x4000x"));
            Assert.AreEqual("c4000c", Store.Get<String>("x4000x"));
            Assert.AreEqual(1, Store.CountInstances("x6000x"));
            Assert.AreEqual("c6000c", Store.Get<String>("x6000x"));
            Assert.AreEqual(1, Store.CountInstances("x8000x"));
            Assert.AreEqual("c8000c", Store.Get<String>("x8000x"));
            Assert.IsFalse(Store.ContainsKey("This Key Doesn't Exist"));
            Assert.AreEqual(0, Store.CountInstances("This Key Doesn't Exist"));       
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