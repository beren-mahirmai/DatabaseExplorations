using System;
using System.Collections.Generic;
using System.Linq;
using Test.Util;
using DBLib;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class ObjectSerializerTests
    {
        private DataGen dataGen = new DataGen();

        [Test]
        public void SerializeInteger()
        {
            ObjectSerializer serializer = new ObjectSerializer();
            Int32 input = 12345;
            Byte[] serializedBytes = serializer.Serialize(input);
            Int32 output = serializer.Deserialize<Int32>(serializedBytes);
            Assert.AreEqual(input, output);
        }

        [Test]
        public void SerializeString() 
        {
            ObjectSerializer serializer = new ObjectSerializer();
            String input = "ABC\n\t\\{}[]xyz";
            Byte[] serializedBytes = serializer.Serialize(input);
            String output = serializer.Deserialize<String>(serializedBytes);
            Assert.AreEqual(input, output);
        }
    }
}