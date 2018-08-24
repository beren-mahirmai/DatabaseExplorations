using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DBLib.Exceptions;

namespace DBLib
{
    public class KeyValueStore2
    {
        // The name of the file data will be written to.
        private const String DataFileName = "./data2.dat";

        // The name of the file used while compacting data.
        private const String TempFileName = "./tempData2.dat";

        // Handles to the data file and temp file
        private FileStream DataFileStream;

        // A BinarySearchTree to use as the short term cache
        private BinarySearchTree Cache;

        // Maximum size of cache before writing to disk
        private const Int32 DataPageSize = 1024;

        // A utility for serializing data
        private readonly ObjectSerializer Serializer = new ObjectSerializer();

        // Searches for the value for the key.  First in checks the in-memory cache, then it
        // looks in the data file.  The file is made up of multiple data pages.  The most recent
        // data pages are at the end of the file, so it reads backward from the end to see the
        // most recent pages first.
        public T Get<T>(String key) {  
            try {
                return Cache.Get<T>(key);
            } catch(DataNotFoundException) {
                // no op here, just continue execution
            } catch (Exception) {
                throw;
            }

            T result = default(T);
            Boolean found = false;
            Int32 i = 0;
            ForEachDataPage(DataFileStream, dataPage => {
                Console.WriteLine($"Exampining data page {i} for {key}");
                try {
                    result = dataPage.Get<T>(key);
                    found = true;
                    Console.WriteLine($"result: {result}, found: {found}");
                    return false; // do not contiue, result was found
                } catch(DataNotFoundException) {
                    Console.WriteLine("continuing search");
                    return true; // continue looping, we haven't found the result yet
                } catch(Exception) {
                    throw;
                }
            });
            if(found) {
                return result;
            }
            throw new DataNotFoundException(key);
        }

        // This method is instrumentation so we can determine if the data is compacted correctly.
        public Int32 CountInstances(String key) {
            Int32 count = 0;
            if(Cache.Contains(key)) count++;

            ForEachDataPage(DataFileStream, dataPage => {
                if(dataPage.Contains(key)) {
                    count++;
                }
                return true; // always continue the loop, we want to look for all occurances
            });

            return count;
        }

        // Adds a new entry to the store.  It adds it to the in-memory cache, then if the cache
        // is too large writes the cache to the disk.
        public void Set(String key, Object value) {
            Cache.Set(key, value);
            if(Cache.Count >= DataPageSize) {
                // dump the current data to the file and start a new in-memory cache
                var dataPage = new ImmutableDataPage(Cache);
                WriteDataPage(dataPage);
                Cache = new BinarySearchTree();
            }
        }

        // Writes a data page to the data file
        public void WriteDataPage(ImmutableDataPage page) {
            Byte[] dataPageBytes = Serializer.Serialize(page);
            Byte[] lengthBytes = BitConverter.GetBytes(dataPageBytes.Length);

            DataFileStream.Position = DataFileStream.Length;
            DataFileStream.Write(dataPageBytes, 0, dataPageBytes.Length);
            DataFileStream.Write(lengthBytes, 0, lengthBytes.Length);
            DataFileStream.Flush();
        }

        // This method takes a function which is executed for each data page in the file.  The most recent
        // data pages are at the end of the file, so it starts and the end and iterates backwards to the start
        // of the file.
        // If the function returns true, continue iteration through the records.
        // If the function returns false, stop iteration.
        private void ForEachDataPage(FileStream stream, Func<ImmutableDataPage, Boolean> action) {
            // This arrays get recycled as we read each record in the file  
            Byte[] lengthBytes = new Byte[4];

            // We don't know how long the data pages are, so we reuse the reference by reallocate the
            // space for each page.
            Byte[] dataPageBytes;

            // Move to the end of the file
            stream.Position = stream.Length;
            
            while(stream.Position > 4) {
                // Backup 4 bytes to get to the beginning of the length value
                stream.Position -= 4;
    
                // Read the length
                stream.Read(lengthBytes, 0, 4);
                Int32 length = BitConverter.ToInt32(lengthBytes, 0);

                // Backup to the beginning of the data page in the file
                stream.Position -= (4 + length);

                // Read the bytes for the data page into an array and deserialize to get the object
                dataPageBytes = new Byte[length];
                stream.Read(dataPageBytes, 0, length);
                ImmutableDataPage dataPage = Serializer.Deserialize<ImmutableDataPage>(dataPageBytes);

                // Run the action against the data page and get the "keepReading" boolean
                Boolean keepReading = action(dataPage);

                // If we don't need to keep reading, stop
                if(!keepReading) break;

                // Back up to the beginning of the record, which we can do now that we know the length
                stream.Position -= length;
            }
        }

        // Removes any existing data file and opens a new empty file stream.
        public void Initialize() {
            Clear();
            Cache = new BinarySearchTree();
            DataFileStream = File.Create(DataFileName);
            Console.WriteLine(DataFileStream.Name);
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

        // Determine if the key exists in the store.
        public Boolean ContainsKey(String key) {
            return this.CountInstances(key) > 0;
        }
    }
}
