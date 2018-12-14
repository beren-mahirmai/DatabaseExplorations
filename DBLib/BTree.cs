using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBLib.Exceptions;

namespace DBLib
{
    internal interface IBTree {
        Int32 StartRange { get; }

        Int32 EndRange { get; }

        Int32 Count { get; }

        void Set(String newKey, Object newValue); 

        Boolean Contains(String key); 

        T Get<T>(String key); 
    }

    public class BTree : IBTree {

        public Int32 StartRange => 
            Content.Min(bt => bt.StartRange);

        public Int32 EndRange => 
            Content.Max(bt => bt.EndRange);

        public Int32 Count => 
            Content.Sum(bt => bt.Count);

        private IBTree[] Content => 
            ContentArray.Where(bt => bt != null).ToArray();

        public BTree() { }
        
        private IBTree[] ContentArray = new IBTree[32];

        public void Set(String newKey, Object newValue) {
            Int32 hashCode = newKey.GetHashCode();
            IBTree containingBTree = Content.FirstOrDefault(bt => bt.StartRange <= hashCode && bt.EndRange >= hashCode);
            if(containingBTree != null) {
                containingBTree.Set(newKey, newValue);
            }

            IEnumerable<IBTree> preceedingContent = Content.Where(bt => bt.EndRange < hashCode);
            IEnumerable<IBTree> followingContent = Content.Where(bt => bt.StartRange > hashCode);

            // TODO: Finishing build the content array here
        }

        public Boolean Contains(String key) {
            return Content.Any(bt => bt.Contains(key));
        }

        public T Get<T>(String key) {
            foreach(IBTree bt in Content) {
                T x = bt.Get<T>(key);
                if(!x.Equals(default(T))) return x;
            }
            return default(T);
        }
    }

    internal class ValueNode : IBTree
    {
        private Int32 KeyHash;

        private Object Value;

        public Int32 StartRange => KeyHash;

        public Int32 EndRange => KeyHash;

        public int Count => 1;

        public ValueNode(String key, Object value) {
            Set(key, value);
        }

        public bool Contains(String key)
        {
            Int32 hash = key.GetHashCode();
            return KeyHash == hash;
        }

        public T Get<T>(String key)
        {
            if(!Contains(key)) {
                return default(T);
            }
            return (T) Value;
        }

        public void Set(String newKey, Object newValue)
        {
            KeyHash = newKey.GetHashCode();
            Value = newValue;
        }
    }
}