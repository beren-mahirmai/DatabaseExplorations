using System;
using System.Collections;
using System.Collections.Generic;

namespace DBLib
{
    // This is a recursive algorithm, but recursion is bad in languages without tail call optimization.
    // We're using an iterative approach to avoid OutOfMemoryException in very large sets.
    public class BinarySearchTree<T>
    {
        private readonly IComparer<T> Comparer;

        private Node Root = null;

        public Int32 Count { get; private set; } = 0;

        public BinarySearchTree(IComparer<T> comparer) {
            Comparer = comparer;
        }

        public void Add(T newValue) {
            Count++;
            if(Root == null) {
                Root = new Node(newValue);
                return;
            } 
            Node current = Root;
            while(true) {
                if(Comparer.Compare(current.Value, newValue) > 0) {
                    // current.Value > value
                    if(current.LowerValues == null) {
                        current.LowerValues = new Node(newValue);
                        break;
                    } else {
                        current = current.LowerValues;
                        continue;
                    }
                } else {
                    // current.Value <= value
                    if(current.HigherValues == null) {
                        current.HigherValues = new Node(newValue);
                        break;
                    } else {
                        current = current.HigherValues;
                        continue;
                    }
                }
            }
        }

        public Boolean Contains(T value) {
            Node current = Root;
            while(true) {
                Int32 compareResult = Comparer.Compare(current.Value, value);
                if(compareResult == 0) {
                    return true;
                } else if(compareResult > 0) {
                    // current.Value > value
                    if(current.LowerValues == null) {
                        return false;
                    } else {
                        current = current.LowerValues;
                        continue;
                    }
                } else {
                    // current.Value <= value
                    if(current.HigherValues == null) {
                        return false;
                    } else {
                        current = current.HigherValues;
                        continue;
                    }
                }
            }
        }

        public T[] ToSortedArray() {
            return new T[0];
            // T[] result = new T[Count];
            // if(Root == null) {
            //     return result;
            // }
            // Int32 index = 0;
            // Node current = Root;
            // while(true) {
                
            // }
        }

        private class Node {
            public T Value;
            public Node LowerValues;
            public Node HigherValues;

            public Node(T value) {
                Value = value;
            }
        }
    }
}
