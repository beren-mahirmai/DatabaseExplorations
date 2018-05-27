using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace DBLib
{
    // Serializes any object to a byte array suitable for writing to a file.
    public sealed class ObjectSerializer 
    {
        // Converts each object to a sequence of bytes suitable for writing to a binary file.
        public Byte[] Serialize(Object obj) 
        {
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
        public T Deserialize<T>(Byte[] bytes) {
            T result;
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream(bytes))
            {
                 result = (T)formatter.Deserialize(ms);                         
            }
            return result;
        }
    }
}