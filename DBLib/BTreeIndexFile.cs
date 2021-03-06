using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DBLib.Exceptions;

namespace DBLib
{
    public class BTreeIndexFile {

        private class DataPage {

            // A utility for serializing data
            private static ObjectSerializer Serializer = new ObjectSerializer();

            public readonly Int32 Offset;

            // For a value node n, the child node n comes before it, and n+1 comes after it.
            public readonly Int32[] ValueOffset; // length = n
            public readonly Int32[] ChildOffset; // length = n + 1

            public DataPage(Int32 offset) {
                Offset = offset;
                ValueOffset = new Int32[BranchingFactor - 1];
                ChildOffset = new Int32[BranchingFactor];
            }

            // The number of bytes to read or write is constant depending on the BranchingFactor,
            // so pre-calculate that here.
            private const Int32 ValueByteLength = 4 * (BranchingFactor -1);
            private const Int32 ChildByteLength = 4 * BranchingFactor;

            public DataPage(FileStream fileStream, Int32 offset) {
                Offset = offset;
                Byte[] valueBytes = new Byte[ValueByteLength];
                Byte[] childBytes = new Byte[ChildByteLength];
                fileStream.Position = offset;
                fileStream.Read(valueBytes, 0, ValueByteLength);
                fileStream.Read(childBytes, 0, ChildByteLength);
                ValueOffset = Serializer.Deserialize<Int32[]>(valueBytes);
                ChildOffset = Serializer.Deserialize<Int32[]>(childBytes);
            }

            public void WriteToFile(FileStream fileStream) {
                Byte[] valueBytes = Serializer.Serialize(ValueOffset);
                Byte[] childBytes = Serializer.Serialize(ChildOffset);
                fileStream.Position = Offset;
                fileStream.Write(valueBytes, 0, valueBytes.Length);
                fileStream.Write(childBytes, 0, childBytes.Length);
                fileStream.Flush();
            }
        }

        private FileStream IndexFileStream;

        public Int32 Count {
            get {
                throw new NotImplementedException();
            }
        } 

        private const Int32 BranchingFactor = 32; 

        public BTreeIndexFile(FileStream indexFileStream) {
            IndexFileStream = indexFileStream;
        }
        
        public void Set(String key, Int32 value) {
            Int32 hash = key.GetHashCode();
            throw new NotImplementedException();
        }

        public T Get<T>(String key) {
            Int32 hash = key.GetHashCode();
            throw new NotImplementedException();
        }

        public Boolean Contains(String key) {
            Int32 hash = key.GetHashCode();
            throw new NotImplementedException();
        }
    }
}