using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace Test.Util
{
    public class DataGen
    {
        Faker DataFaker = new Faker();

        // Append a counter to each output, because Bogus can generate duplicate data.
        private Int64 dataCounter = 0;

        public List<String> GenStrings(Int32 size) {
            return Enumerable.Range(0, size).Select(i => GenString()).ToList();
        }

        public String GenString() {
            dataCounter++;
            return DataFaker.Hacker.Phrase() + dataCounter;
        }

        public String GenWord() {
            dataCounter++;
            return DataFaker.Hacker.Noun() + dataCounter;
        }

        public List<KeyValuePair<String, String>> GenKeyValuePairs(Int32 size) {
            return Enumerable.Range(0, size).Select(i => new KeyValuePair<String, String>(GenWord(), GenString())).ToList();
        }
    }
}