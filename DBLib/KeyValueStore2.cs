using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DBLib
{
    public class KeyValueStore2
    {
        // The name of the file data will be written to.
        private const String DataFileName = "./data.dat";

        // The name of the file used while compacting data.
        private const String TempFileName = "./tempData.dat";

        // Handles to the data file and temp file
        private FileStream DataFileStream;

        // A BinarySearchTree to use as the short term cache
        private BinarySearchTree Cache;

        // Maximum size of cache before writing to disk
        private const Int32 DataPageSize = 1024;

        public T Get<T>(String key) {  
            return Cache.Get<T>(key);
        }

        // Converts each object to a sequence of bytes suitable for writing to a binary file.
        private static Byte[] SerializeObject(Object obj) {
            Byte[] bytes;
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                bytes = ms.GetBuffer();
            }
            return bytes;
        }

        // Converts a sequence of bytes into an Object of the requested type.
        private static T DeserializeObject<T>(Byte[] bytes) {
            T result;
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream(bytes))
            {
                 result = (T)formatter.Deserialize(ms);                         
            }
            return result;
        }

        // Appends a new entry to the end of the file
        public void Set(String key, Object value) {
            Cache.Set(key, value);
            // if(Cache.Count >= DataPageSize) {
            //     KeyValuePair<String, Object>[] sortedArray = Cache.ToSortedArray();
            //     var dataPage = new ImmutableDataPage(sortedArray);
            // }
        }

        // Remember that our data file consists of records formatted like this:
        //  <value, variable length><length of value, 4 bytes><hash of the key, 4 bytes>
        // We only ever append to the file since that is very quick, and we will always know
        // which value is most recent if a hash appears twice because the more recent value
        // will appear later in the file.  So to write a record just move to the end of the
        // file and write the bytes in the required order.
        private void WriteRecord(Int32 hash, Byte[] valueBytes) {
            throw new NotImplementedException();
        }

        // Removes any existing data file and opens a new empty file stream.
        public void Initialize() {
            Clear();
            Cache = new BinarySearchTree();
            DataFileStream = File.Create(DataFileName);
        }

        // Closes the file stream and removes any existing file from the disk.
        public void Clear() {
            DataFileStream?.Close();
            if(File.Exists(DataFileName)) {
                File.Delete(DataFileName);
            }
        }

        // Retrieve a summary of the data file's contents as a dictionary.
        //   Key: the hash of a key
        //   Values: the number of times the key appears in the database
        // Only the most recent value is ever retrieved from the database normally, but this gives us 
        // some idea how much compacting work there is to do.  Notice that we can't actually retrieve
        // the keys things were stored with, we only know the hashes.
        public Dictionary<Int32, Int32> GetStats() {
            throw new NotImplementedException();
        }

        // Since we only append to the file when data is modified, which is fast, orphaned older values
        // for keys are left behind.  The compact process removes the old values from the file.
        // -Close the data file and rename it to a temp file name
        // -Create a new data file
        // -Go through the old data file and copy only the most recent data from it into the new file
        public void Compact() {
            throw new NotImplementedException();
        }

        public Boolean ContainsKey(String key) {
            return Cache.Contains(key);
        }
    }
}
