using System;
using System.Collections;
using System.Collections.Generic;
using DBLib.Exceptions;

namespace DBLib
{
    internal interface IBTree {
        Int32 Count { get; }

        void Set(String newKey, Object newValue); 

        Boolean Contains(String key); 

        T Get<T>(String key); 

        KeyValuePair<String, Object>[] ToSortedArray(); 
    }

    public class BTree : IBTree {

        public Int32 Count { get; private set; } = 0;

        private IBTree[] Contents = new IBTree[32];

        public BTree() { }

        public void Set(String newKey, Object newValue) {
            throw new NotImplementedException();
        }

        public Boolean Contains(String key) {
            throw new NotImplementedException();
        }

        public T Get<T>(String key) {
            throw new NotImplementedException();
        }

        public KeyValuePair<String, Object>[] ToSortedArray() {
            throw new NotImplementedException();
        }
    }

    internal class ValueNode : IBTree
    {
        private String Key;

        private Object Value;

        public int Count => 1;

        public bool Contains(String key)
        {
            return Key == key;
        }

        public T Get<T>(String key)
        {
            if(!Contains(key)) {
                throw new DataNotFoundException(key);
            }
            return (T) Value;
        }

        public void Set(String newKey, Object newValue)
        {
            Key = newKey;
            Value = newValue;
        }

        public KeyValuePair<String, Object>[] ToSortedArray()
        {
            return new KeyValuePair<String, Object>[] {
                new KeyValuePair<String, Object>(Key, Value)
            };
        }
    }
}