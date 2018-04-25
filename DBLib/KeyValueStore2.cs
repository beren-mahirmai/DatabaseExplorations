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

        public T Get<T>(String key) {  
            Int32 requestedHash = key.GetHashCode();
            T result = default(T);
            Boolean valueFound = false;
            ForEachRecord(DataFileStream, (hash, getValueBytes) => {
                if(hash != requestedHash) return true; // continue looking
                Byte[] valueBytes = getValueBytes();
                result = DeserializeObject<T>(valueBytes);
                valueFound = true;
                return false; // stop iteration, we found what we need
            });
            if(!valueFound) {
                throw new Exception($"'{key}' not found");
            }
            return result;
        }

        // This method takes a function which is executed for each record, with...
        // -The hash of the record
        // -An accessor function that returns the bytes of the value
        // If the function returns true, continue iteration through the records.
        // If the function returns false, stop iteration.
        private void ForEachRecord(FileStream stream, Func<Int32, Func<Byte[]>, Boolean> action) {
            // These arrays get recycled as we read each record in the file  
            Byte[] lengthBytes = new Byte[4];
            Byte[] hashBytes = new Byte[4];

            // Move to the end of the file
            stream.Position = stream.Length;
            
             while(stream.Position > 8) {
                // Backup 8 bytes, space for two Int32 values: hash and length
                stream.Position -= 8;
    
                // Length comes first, is this is the length of the data
                stream.Read(lengthBytes, 0, 4);
                Int32 length = BitConverter.ToInt32(lengthBytes, 0);

                // Hash comes next, this is the identifies for the data
                stream.Read(hashBytes, 0, 4);
                Int32 hash = BitConverter.ToInt32(hashBytes, 0);
                Boolean keepReading = action(hash, () => {
                    // Back up to the beginning of the record, which we can do now that we know the length
                    stream.Position -= (length + 8);
                    Byte[] valueBytes = new Byte[length];
                    stream.Read(valueBytes, 0, length);
                    stream.Position += 8; // reset the position to where it was before we read the value
                    return valueBytes;
                });

                if(!keepReading) break;

                // Back up to the beginning of the record, which we can do now that we know the length
                stream.Position -= (length + 8);
            }
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
            Int32 hash = key.GetHashCode();
            Byte[] valueBytes = SerializeObject(value);
            WriteRecord(hash, valueBytes);
        }

        // Remember that our data file consists of records formatted like this:
        //  <value, variable length><length of value, 4 bytes><hash of the key, 4 bytes>
        // We only ever append to the file since that is very quick, and we will always know
        // which value is most recent if a hash appears twice because the more recent value
        // will appear later in the file.  So to write a record just move to the end of the
        // file and write the bytes in the required order.
        private void WriteRecord(Int32 hash, Byte[] valueBytes) {
            Byte[] hashBytes = BitConverter.GetBytes(hash);
            Byte[] lengthBytes = BitConverter.GetBytes(valueBytes.Length);

            DataFileStream.Position = DataFileStream.Length;
            DataFileStream.Write(valueBytes, 0, valueBytes.Length);
            DataFileStream.Write(lengthBytes, 0, lengthBytes.Length);
            DataFileStream.Write(hashBytes, 0, hashBytes.Length);
            DataFileStream.Flush();
        }

        // Removes any existing data file and opens a new empty file stream.
        public void Initialize() {
            Clear();
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
            var stats = new Dictionary<Int32, Int32>();

            // Iterate through each record, making note of how many times each hash code appears.
            ForEachRecord(DataFileStream, (hash, getValueBytes) => {
                if(!stats.ContainsKey(hash)) {
                    stats[hash] = 0;
                }
                stats[hash]++;
                return true; // always continue, we want to see all the records
            });
            return stats;
        }

        // Since we only append to the file when data is modified, which is fast, orphaned older values
        // for keys are left behind.  The compact process removes the old values from the file.
        // -Close the data file and rename it to a temp file name
        // -Create a new data file
        // -Go through the old data file and copy only the most recent data from it into the new file
        public void Compact() {
            DataFileStream.Close();
            // Just to be safe, look for the temp file name and if one already exists delete it.
            if(File.Exists(TempFileName)) {
                File.Delete(TempFileName);
            }
            File.Move(DataFileName, TempFileName); // backup the contents of the data file to temp
            Initialize(); // creates a new instance of the data file

            // Keep a list of hashes we've already seen, so we know not to copy their values again
            var previousHashes = new List<Int32>();
            using(FileStream tempFileStream = File.Open(TempFileName, FileMode.Open)) {
                // The first time we see each hash is the most recent value for it, so we want to copy
                // it to the new data file and never copy any other values we see for that hash later.
                ForEachRecord(tempFileStream, (hash, getValueBytes) => {
                    if(previousHashes.Contains(hash)) return true;
                    previousHashes.Add(hash);
                    WriteRecord(hash, getValueBytes());
                    return true;
                });
            }

            // Delete the temp file since we don't need it anymore
            File.Delete(TempFileName);
        }

        public Boolean ContainsKey(String key) {
            Int32 expectedHash = key.GetHashCode();
            Boolean result = false;
            ForEachRecord(DataFileStream, (hash, getValueBytes) => {
                if(hash == expectedHash) {
                    result = true;
                    return false; // stop looking
                }
                return true; // continue looking
            });
            return result;
        }
    }
}
