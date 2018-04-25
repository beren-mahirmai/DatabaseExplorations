using System;
using System.Collections.Generic;
using System.Linq;

namespace DBLib
{
    public class BloomFilter
    {
        // The size of the bit array.
        private Int32 Size;

        // The number of integer addresses generated (and bits set) for each added item.
        private Int32 NumHashes;

        // The bit array, each stored object will correspond to NumHashes number of bits
        // that will be set in the array.
        private Boolean[] Bits;

        public BloomFilter(Int32 size, Int32 numHashes) {
            Size = size;
            NumHashes = numHashes;
            Bits = new Boolean[size];
        }

        // Gets a set of addresses in the bit array that match the hash of the object,
        // then sets them.
        public void Add(Object x) {
            IEnumerable<Int32> addresses = GetBitAddresses(x);
            addresses
                .ToList() // just so I can use .ForEach()
                .ForEach(addr => Bits[addr % Size] = true);
        }

        // If any of the address in the bit array are false that are expected to be true,
        // then the item couldn't have been added to the filter.
        public Boolean Contains(Object x) {
            IEnumerable<Int32> addresses = GetBitAddresses(x);
            return !addresses.Any(addr => Bits[addr % Size] == false);
        }

        // Gets the set of bit address that should be set for a given object.
        private IEnumerable<Int32> GetBitAddresses(Object x) {
            Int32 seed = x.GetHashCode();
            var random = new Random(seed);
            return Enumerable
                .Range(0, NumHashes)
                .Select(i => random.Next());
        }
    }
}
