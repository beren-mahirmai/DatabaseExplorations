using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DBLib.Exceptions;

namespace DBLib
{
    public class KeyValueStore3
    {
        // The name of the file data will be written to.
        private const String DataFileName = "./data3.dat";

        // The name of the file used while compacting data.
        private const String TempFileName = "./tempData3.dat";

        // For simplicity I'm keeping the file's index in a seperate file.
        private const String IndexFileName = "./index3.dat";

        // Handles to the data file and temp file
        private FileStream DataFileStream;

        public T Get<T>(String key) {  
            throw new NotImplementedException();
        }

        public Int32 CountInstances(String key) {
            throw new NotImplementedException();
        }

        public void Set(String key, Object value) {
            throw new NotImplementedException();
        }

        public void Initialize() {
            throw new NotImplementedException();
        }
            
        public void Clear() {
            throw new NotImplementedException();
        }

        public void Compact() {
            throw new NotImplementedException();
        }

        public Boolean ContainsKey(String key) {
            throw new NotImplementedException();
        }
    }
}