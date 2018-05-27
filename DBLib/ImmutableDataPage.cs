using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBLib.Exceptions;

namespace DBLib
{
    // A data structure that is built once and never modified.  It consists of a sorted array
    // of key/value pairs and a bloom filter describing the keys in the array.  The idea is
    // that when we request the value for a key we can quickly check the bloom filter and verify
    // if we even have it in the data.  If we do, since the data is sorted, we can use a binary
    // search to find it very quickly.
    // The intent is for this structure to be serialized to disk, and then quickly read when a
    // value is being sought.
    [Serializable]
    public class ImmutableDataPage
    {
        private readonly IComparer<String> Comparer = StringComparer.CurrentCultureIgnoreCase;

        // Comparing string is expensive, so the BloomFilter of the keys potentially saves us
        // from having to make the comparisons involved in the binary search.
        private readonly BloomFilter Filter = new BloomFilter(8192, 3);

        // This data is pre-sorted, so we can efficiently find elements using binary search.
        private readonly KeyValuePair<String, Object>[] Data;

        public Int32 Count => Data.Count();

        // We build this structure from a BinarySearchTree, which can take elements input in a
        // random order and easily return them in a sorted order.  The intent is for the tree
        // to act as a cache of recent changes, while this immutable structure can more be stored
        // on disk.
        public ImmutableDataPage(BinarySearchTree inputTree) {
            Data = inputTree.ToSortedArray();
            Data.Select(kvp => kvp.Key)
                .ToList()
                .ForEach(key => Filter.Add(key));
        }

        // This looks for the value using two speed-enhancing techniques.
        // -The BloomFilter saves us from having to look for non-existant data
        // -The data itself is pre-sorted, so we can use binary search to find elements quickly
        public T Get<T>(String key) {
            if(!Filter.Contains(key)) {
                throw new DataNotFoundException(key);
            }

            Int32 windowStart = 0;
            Int32 windowEnd = Data.Length;
            while(windowEnd - windowStart > 0) {
                Int32 middle = Convert.ToInt32(Math.Floor(Convert.ToDecimal((windowStart + windowEnd) / 2)));
                String x = Data[middle].Key;
                Int32 compareResult = Comparer.Compare(key, x);
                if(compareResult == 0) {
                    return (T)Data[middle].Value;
                } else if(compareResult > 0) {
                    windowStart = middle;
                } else {
                    windowEnd = middle;
                }
            }
            // BloomFilters can give false positives, so we might end up here.
            return default(T);
        }
    }
}