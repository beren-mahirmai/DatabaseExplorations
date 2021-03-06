using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBLib.Exceptions;

namespace DBLib
{
    public class BTree {

        public Int32 Count {
            get {
                Int32 localCount = ValueNodes.Count(n => n != null);
                Int32 childCount = ChildNodes.Where(n => n != null).Sum(n => n.Count);
                return localCount + childCount; 
            }
        } 

        private const Int32 BranchingFactor = 32; 

        public BTree() { }
        
        // For a value node n, the child node n comes before it, and n+1 comes after it.
        private ValueNode[] ValueNodes = new ValueNode[BranchingFactor -1];
        private BTree[] ChildNodes = new BTree[BranchingFactor];

        public void Set(String newKey, Object newValue) {
            Int32 newKeyHash = newKey.GetHashCode();

            SeekResult seek = Seek(newKeyHash);

            if(seek.StepType == StepType.Value) {
                ValueNodes[seek.Index] = new ValueNode(newKeyHash, newValue);
                return;
            }

            if(ChildNodes[seek.Index] == null) {
                ChildNodes[seek.Index] = new BTree();
            }
            ChildNodes[seek.Index].Set(newKey, newValue);
        }

        private SeekResult Seek(Int32 hash) {
            Int32 index = -1;
            StepType stepType = StepType.Unknown;
            for(var i=0; i<ValueNodes.Length; i++) {
                if(ValueNodes[i] == null) {
                    index = i;
                    stepType = StepType.Value;
                    break;
                }
                ValueNode kvp = ValueNodes[i];
                if(kvp.Key == hash) {
                    index = i;
                    stepType = StepType.Value;
                    break;
                }
                if(kvp.Key > hash) {
                    index = i;
                    stepType = StepType.Child;
                    break;
                }
                // kvp.Key < newKeyHash, so continue loop
            }

            if(index == -1) {
                // This means we never found a key greater than newKey, so we insert at
                // the last child node
                index = ChildNodes.Length;
                stepType = StepType.Child;
            }

            return new SeekResult(index, stepType);
        }

        public T Get<T>(String key) {
            Int32 hash = key.GetHashCode();

            SeekResult seek = Seek(hash);

            if(seek.StepType == StepType.Value) {
                if(ValueNodes[seek.Index] == null) {
                    throw new DataNotFoundException(key);
                }
                return (T) ValueNodes[seek.Index].Value;
            }

            if(ChildNodes[seek.Index] == null) {
               throw new DataNotFoundException(key); 
            }
            return ChildNodes[seek.Index].Get<T>(key);
        }

        public Boolean Contains(String key) {
             Int32 hash = key.GetHashCode();

            SeekResult seek = Seek(hash);

            if(seek.StepType == StepType.Value) {
                return ValueNodes[seek.Index] != null;
            }

            if(ChildNodes[seek.Index] == null) {
               return false;
            }
            return ChildNodes[seek.Index].Contains(key);
           
        }

        private class ValueNode {
            public readonly Int32 Key;
            public readonly Object Value;

            public ValueNode(Int32 key, Object value) {
                Key = key;
                Value = value;
            }
        }

        private class SeekResult {
            public readonly Int32 Index = -1;
            public readonly StepType StepType;

            public SeekResult(Int32 index, StepType stepType) {
                Index = index;
                StepType = stepType;
            }
        }

        private enum StepType {
            Unknown, // this should never actually be used
            Value,
            Child
        }
    }
}