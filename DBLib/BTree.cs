using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBLib.Exceptions;

namespace DBLib
{
    public class BTree {

        public Int32 Count => 
            throw new NotImplementedException();

        private const Int32 BranchFactor = 33; // odd branching factor, so we can begin an end with value nodes

        public BTree() { }
        
        // Format:
        // -KeyValuePair<Int32, Object>
        // -BTree
        // -KeyValuePair<Int32, Object>
        // ...
        // KeyValuePair on even indexed nodes (0, 2, 4, etc)
        // Sub-trees on odd indexed nodes (1, 3, 5, etc)
        // Begin and end with a KeyValuePair
        private Object[] Content = new Object[BranchFactor];

        public void Set(String newKey, Object newValue) {
            Int32 newKeyHash = newKey.GetHashCode();

            // To start, just see if this BTree contains the exact key, in that case just replace the value.
            // Find out how much of the array is below the new value.
        }

        // A negative return value means the entire array comes AFTER the new key.
        // A positive result means that index, and everything before it, comes before the new key.
        private Int32 FindLowerValueCutoff(Int32 hash) {

            for(var i=0; i<BranchFactor; i+=2) {
                if(Content[i] == null) return i;
                var kvp = (KeyValuePair<Int32, Object>) Content[i];
                if(kvp.Key > hash) {
                    return i-2;
                }
            }
            return BranchFactor; // all of the array was below the new key
        }
        public Boolean Contains(String key) {
            throw new NotImplementedException();
        }

        public T Get<T>(String key) {
            throw new NotImplementedException();
        }
    }
}